using System;
using System.Text;
using System.Threading;
using RSG;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;

namespace Chromia.PostchainClient
{
    public class Restclient
    {
        private string _urlBase;
        private string _blockhainRID;

        public Restclient(string urlBase, string blockhainRID)
        {
            this._urlBase = urlBase;
            this._blockhainRID = blockhainRID;
        }

        public void getTransaction(string messageHash, Action<string, dynamic> callback)
        {
            _validateMessageHash(messageHash);

            _get(this._urlBase, "tx/" + this._blockhainRID + "/" + StringToHex(messageHash), (string error, int statusCode, dynamic responseObject) => 
            {
                _handleGetResponse(error, statusCode, statusCode == 200 ? StringToHex(responseObject["tx"].ToString()) : null, callback);
            });
        }

        public void postTransaction(string serializedTransaction, Action<string, dynamic> callback)
        {
            string jsonString = @"{tx: " + StringToHex(serializedTransaction) + "}";
            var jsonObject = JsonConvert.DeserializeObject<dynamic>(jsonString);

            _doPost(this._urlBase, "tx/" + this._blockhainRID, jsonObject, callback);
        }

        public void getConfirmationProof(string messageHash, Action<string, string> callback)
        {
            _validateMessageHash(messageHash);

            _get(_urlBase, "tx/" + this._blockhainRID + "/" + StringToHex(messageHash) + "/confirmationProof", (string error, int statusCode, dynamic responseObject) => 
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

        public void status(string messageHash, Action<string, dynamic> callback)
        {
            _validateMessageHash(messageHash);

            _get(this._urlBase, "tx/" + this._blockhainRID + "/" + StringToHex(messageHash) + "/status", (string error, int statusCode, dynamic responseObject) => 
            {
                _handleGetResponse(error, statusCode, responseObject, callback);
            });
        }

        public Promise<dynamic> query(string queryName, dynamic queryObject)
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

                _doPost(this._urlBase, "query/" + this._blockhainRID, queryObject, cb);
            });
        }

        public Promise<string> waitConfirmation(string txRID)
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
                                this.waitConfirmation(txRID);
                                break;
                            default:
                                Console.WriteLine(status);
                                reject(new System.Exception("got unexpected response from server"));
                                break;
                        }
                    }
                };

                this.status(txRID, cb);
            });
        }

        public Promise<Promise<string>> postAndWaitConfirmation(string serializedTransaction, string txRID, bool validate)
        {
            if (validate)
            {
                return null;
            }

            return new Promise<Promise<string>>((resolve, reject) => 
            {
                this.postTransaction(serializedTransaction, (err, responseCallback) => 
                {
                    if (err != "")
                    {
                        reject(new System.Exception(err));
                    } else 
                    {
                        resolve(this.waitConfirmation(txRID));
                    }
                });
            });
        }

        private void _doPost(string config, string path, object jsonObject, Action<string, dynamic> responseCallback)
        {
            _post(config, path, jsonObject, (error, statusCode, responseObject) => 
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
                        var jsonString = JsonConvert.SerializeObject(jsonObject);
                        Console.WriteLine("Ok calling responseCallback with responseObject: {0}", jsonString);
                        responseCallback("", responseObject);
                    } catch (Exception e)
                    {
                        Console.WriteLine("restclient.doPost(): Failed to call callback function {0}", e);
                    }
                }
            });
        }

        private async void _get(string urlBase, string path, Action<string, int, dynamic> callback)
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

        private async void _post(string urlBase, string path, object jsonBody, Action<string, int, string> callback)
        {
            var url = Url.Combine(urlBase, path);         
            Console.WriteLine("POST URL {0}", url);

            var response = await url.PostJsonAsync(jsonBody);
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

        private void _validateMessageHash(string messageHash)
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

        private void _handleGetResponse(string error, int statusCode, string responseObject, Action<string, dynamic> callback)
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