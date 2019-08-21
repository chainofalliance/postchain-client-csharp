using System;
using System.Text;
using System.Threading;
using RSG;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;

namespace Chromia.PostchainClient
{
    public class RESTClient
    {
        private string UrlBase;
        private string BlockhainRID;

        public RESTClient(string urlBase, string blockhainRID)
        {
            this.UrlBase = urlBase;
            this.BlockhainRID = blockhainRID;
        }

        public void GetTransaction(string messageHash, Action<string, dynamic> callback)
        {
            ValidateMessageHash(messageHash);

            Get(this.UrlBase, "tx/" + this.BlockhainRID + "/" + messageHash, (string error, int statusCode, dynamic responseObject) => 
            {
                HandleGetResponse(error, statusCode, statusCode == 200 ? responseObject["tx"].ToString(): null, callback);
            });
        }

        public void PostTransaction(string serializedTransaction, Action<string, dynamic> callback)
        {
            string jsonString = String.Format(@"{{""tx"": ""{0}""}}", serializedTransaction);
            
            DoPost(this.UrlBase, "tx/" + this.BlockhainRID, jsonString, callback);
        }

        public void GetConfirmationProof(string messageHash, Action<string, string> callback)
        {
            ValidateMessageHash(messageHash);

            Get(UrlBase, "tx/" + this.BlockhainRID + "/" + messageHash + "/confirmationProof", (string error, int statusCode, dynamic responseObject) => 
            {
                if (statusCode == 200)
                {
                    responseObject["hash"] = _b(responseObject["hash"].ToString());
                    responseObject["blockHeader"] = _b(responseObject["blockHeader"].ToString());
                    if (responseObject["signatures"].ToString() != "")
                    {
                        for (int i = 0; i < responseObject["signatures"].Count; i++)
                        {
                            responseObject["signatures"][i]["pubKey"] = _b(responseObject["signatures"][i]["pubKey"].ToString());
                            responseObject["signatures"][i]["signature"] = _b(responseObject["signatures"][i]["signature"].ToString());
                        }
                    }

                    if (responseObject["merklePath"].ToString() != "")
                    {
                        for (int i = 0; i < responseObject["merklePath"].Count; i++)
                        {
                            responseObject["merklePath"][i]["hash"] = _b(responseObject["merklePath"][i]["hash"].ToString());
                        }
                    }
                }
            });
        }

        public void Status(string messageHash, Action<string, dynamic> callback)
        {
            ValidateMessageHash(messageHash);

            Get(this.UrlBase, "tx/" + this.BlockhainRID + "/" + messageHash + "/status", (string error, int statusCode, dynamic responseObject) => 
            {
                HandleGetResponse(error, statusCode, responseObject, callback);
            });
        }

        public Promise<dynamic> Query(string queryName, dynamic queryObject)
        {
            queryObject.type = queryName;

            return new Promise<dynamic>((resolve, reject) => 
            {
                Action<string, dynamic> cb = delegate(string error, dynamic result)
                {
                    if (error != "")
                    {
                        reject(new System.Exception(error));
                    } else 
                    {
                        resolve(result);
                    }
                };

                DoPost(this.UrlBase, "query/" + this.BlockhainRID, queryObject, cb);
            });
        }

        public Promise<string> WaitConfirmation(string txRID)
        {
            return new Promise<string>((resolve, reject) => 
            {
                Action<string, dynamic> cb = delegate(string error, dynamic result)
                {
                    if (error != "")
                    {
                        resolve(error);
                    } else 
                    {
                        var status = result.status;
                        switch(status)
                        {
                            case "confirmed":
                                resolve(null);
                                break;
                            case "rejected":
                                reject(new System.Exception("Message was rejected"));
                                break;
                            case "unknown":                                
                                reject(new System.Exception("Server lost our message"));
                                break;
                            case "waiting":
                                // I don't think that will work
                                Thread.Sleep(511);
                                this.WaitConfirmation(txRID);
                                break;
                            default:
                                Console.WriteLine(status);
                                reject(new System.Exception("got unexpected response from server"));
                                break;
                        }
                    }
                };

                this.Status(txRID, cb);
            });
        }

        public Promise<Promise<string>> PostAndWaitConfirmation(string serializedTransaction, string txRID, bool validate = false)
        {
            if (validate)
            {
                return null;
            }

            return new Promise<Promise<string>>((resolve, reject) => 
            {
                this.PostTransaction(serializedTransaction, (err, responseCallback) => 
                {
                    if (err != "")
                    {
                        reject(new System.Exception(err));
                    } else 
                    {
                        resolve(this.WaitConfirmation(txRID));
                    }
                });
            });
        }

        private void DoPost(string config, string path, string jsonString, Action<string, dynamic> responseCallback)
        {
            Post(config, path, jsonString, (error, statusCode, responseObject) => 
            {
                if (error != "")
                {
                    Console.WriteLine("In resclient doPost(). " + error);
                } else if (statusCode != 200)
                {
                    Console.WriteLine("Unexpected status code from server: " + statusCode);
                } else
                {
                    try
                    {
                        Console.WriteLine("Ok calling responseCallback with responseObject: {0}", jsonString);
                        responseCallback("", responseObject);
                    } catch (Exception e)
                    {
                        Console.WriteLine("restclient.doPost(): Failed to call callback function {0}", e);
                    }
                }
            });
        }

        private async void Get(string urlBase, string path, Action<string, int, dynamic> callback)
        {
            var url = Url.Combine(urlBase, path);         
            Console.WriteLine("GET URL {0}", url);

            var response = await url.GetAsync();
            dynamic jsonObject = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
            if (!response.IsSuccessStatusCode)
            {
                callback(jsonObject.ToString(), 0, null);
            } else
            {
                try
                {
                    callback("", (int) response.StatusCode, jsonObject);
                } catch (Exception e)
                {
                    callback(e.ToString(), 0, null);
                }
            }
        }

        private async void Post(string urlBase, string path, string jsonString, Action<string, int, string> callback)
        {
            var url = Url.Combine(urlBase, path);         
            Console.WriteLine("POST URL {0}", url);

            var response = await url.PostJsonAsync(JsonConvert.DeserializeObject<object>(jsonString));
            dynamic jsonObject = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
            if (!response.IsSuccessStatusCode)
            {
                callback(jsonObject.ToString(), 0, null);
            } else
            {
                try
                {
                    callback("", (int) response.StatusCode, jsonObject);
                } catch (Exception e)
                {
                    callback(e.ToString(), 0, null);
                }
            }
        }

        private void ValidateMessageHash(string messageHash)
        {
            if (messageHash == null)
            {
                throw new Exception("messageHash is not a Buffer");
            }

            if (messageHash.Length != 32)
            {
                throw new Exception("expected length 32 of messageHash, but got " + messageHash);
            }
        }

        private void HandleGetResponse(string error, int statusCode, string responseObject, Action<string, dynamic> callback)
        {
            if (error == "")
            {
                callback(error, null);
            } else if (statusCode == 404)
            {
                Console.WriteLine("404 received");
                callback("", null);
            } else if (statusCode != 200)
            {
                callback("Unexpected status code from server: " + statusCode, null);
            } else
            {
                try
                {
                    callback("", responseObject);
                } catch (Exception e)
                {
                    Console.WriteLine("restclient.handleGetResponse(): Failed to call callback function " + e);
                }
            }
        }

        private string StringToHex(string stringValue)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in stringValue)
            { 
                sb.Append(Convert.ToInt32(c).ToString("X")); 
            }
            return sb.ToString();
        }

        private string _b(string stringValue)
        {
            int r;
            if(int.TryParse(stringValue, 
                    System.Globalization.NumberStyles.HexNumber, 
                    System.Globalization.CultureInfo.InvariantCulture, out r))
            {
                return stringValue;
            }

            return StringToHex(stringValue);
        }
    }
}