using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;

namespace Chromia.Postchain.Client
{
    internal class HTTPResponse
    {
        public string status = "";
        public string message = "";
    }

    public class RESTClient
    {
        private string UrlBase;
        private string BlockchainRID;

        ///<summary>
        ///Create new RESTClient object.
        ///</summary>
        ///<param name = "urlBase">URL to rest server.</param>
        ///<param name = "blockchainRID">RID of blockchain.</param>
        public RESTClient(string urlBase, string blockchainRID)
        {
            this.UrlBase = urlBase;
            this.BlockchainRID = blockchainRID;
        }

        public async Task<object> PostTransaction(string serializedTransaction)
        {
            string jsonString = String.Format(@"{{""tx"": ""{0}""}}", serializedTransaction);
            
            return await Post(this.UrlBase, "tx/" + this.BlockchainRID, jsonString);
        }

        private async Task<HTTPResponse> Status(string messageHash)
        {
            ValidateMessageHash(messageHash);
            return await Get(this.UrlBase, "tx/" + this.BlockchainRID + "/" + messageHash + "/status");
        }

        public async Task<dynamic> Query(string queryName, params dynamic[] queryObject)
        {
            string queryString = BuildQuery(queryObject);
            queryString = AppendQueryName(queryName, queryString);

            return await Post(this.UrlBase, "query/" + this.BlockchainRID, queryString);
        }

        private string AppendQueryName(string queryName, string queryString)
        {
            if (!String.IsNullOrEmpty(queryString))
            {
                queryString = queryString.Remove(queryString.Length - 1);
                queryString += String.Format(@", ""type"": ""{0}""", queryName);
                return queryString + " }";
            }
            else
            {
                return String.Format(@"{{""type"": ""{0}""}}", queryName);
            }
        }

        private static string BuildQuery(dynamic queryObject, int layer = 0)
        {
            if (IsTuple(queryObject.GetType()))
            {
                if (layer < 2)
                {
                    return String.Format(@"""{0}"": {1}", queryObject.Item1, BuildQuery(queryObject.Item2, layer + 1));
                }
                else
                {
                    string queryString = "[";
                    var queryItems = ToEnumerable(queryObject);
                    foreach (var queryItem in queryItems)
                    {
                        queryString += BuildQuery(queryItem, layer + 1) + ", ";
                    }
                    queryString = queryString.Remove(queryString.Length - 2) + "]";
                    return queryString;
                }
            }
            else if (queryObject is byte[])
            {
                return String.Format(@"""{0}""", PostchainUtil.ByteArrayToString(queryObject));
            }
            else if (queryObject is System.Array)
            {
                if (layer == 0 && queryObject.Length == 0)
                {
                    return "";
                }
                else if(layer != 0 && queryObject.Length == 0)
                {
                    return "[]";
                }
                
                string queryString  = "";
                if(layer == 0)
                {
                    queryString = "{";
                }
                else 
                {
                    queryString = "[";
                }

                foreach (var subQueryParam in queryObject)
                {
                    queryString += BuildQuery(subQueryParam, layer + 1) + ", ";
                }

                if(layer == 0)
                {
                    queryString = queryString.Remove(queryString.Length - 2) + "}";
                }
                else 
                {
                    queryString = queryString.Remove(queryString.Length - 2) + "]";
                }
                return queryString;
            }
            else if (PostchainUtil.IsNumericType(queryObject))
            {
                return queryObject.ToString();
            }
            else if (queryObject is string)
            {
                return String.Format(@"""{0}""", (string) queryObject);
            }
            else
            {
                throw new Exception("Unknown query data type " + queryObject.GetType());
            }
        }

        private static IEnumerable<object> ToEnumerable(object tuple)
        {
            if (IsTuple(tuple.GetType()))
            {
                foreach (var prop in tuple.GetType()
                    .GetFields()
                    .Where(x => x.Name.StartsWith("Item")))
                {
                    yield return prop.GetValue(tuple);
                }
            }
            else
            {
                throw new ArgumentException("Not a tuple!");
            }
        }

        public static bool IsTuple(Type tuple)
        {
            if (!tuple.IsGenericType)
                return false;
            var openType = tuple.GetGenericTypeDefinition();
            return openType == typeof(ValueTuple<>)
                || openType == typeof(ValueTuple<,>)
                || openType == typeof(ValueTuple<,,>)
                || openType == typeof(ValueTuple<,,,>)
                || openType == typeof(ValueTuple<,,,,>)
                || openType == typeof(ValueTuple<,,,,,>)
                || openType == typeof(ValueTuple<,,,,,,>)
                || (openType == typeof(ValueTuple<,,,,,,,>) && IsTuple(tuple.GetGenericArguments()[7]));
        }

        public async Task<PostchainErrorControl> WaitConfirmation(string txRID)
        {
            var response = await this.Status(txRID);

            foreach(System.ComponentModel.PropertyDescriptor descriptor in System.ComponentModel.TypeDescriptor.GetProperties(response))
            {
                string name=descriptor.Name;
                object value=descriptor.GetValue(response);
                Console.WriteLine("{0}={1}",name,value);
            }

            switch(response.status)
            {
                case "confirmed":
                    return new PostchainErrorControl() {Error = false, ErrorMessage = ""};
                case "rejected":
                case "unknown":
                    return new PostchainErrorControl() {Error = true, ErrorMessage = "Message was rejected"};
                case "waiting":
                    await Task.Delay(511);
                    return await this.WaitConfirmation(txRID);
                case "exception":
                    return new PostchainErrorControl() {Error = true, ErrorMessage = "HTTP Exception: " + response.message};
                default:
                    return new PostchainErrorControl() {Error = true, ErrorMessage = "Got unexpected response from server: " + response.status};
            }
        }

        public async Task<PostchainErrorControl> PostAndWaitConfirmation(string serializedTransaction, string txRID)
        {
            await this.PostTransaction(serializedTransaction);

            return await this.WaitConfirmation(txRID);
        }

        private async Task<HTTPResponse> Get(string urlBase, string path)
        {
            try
            {
                var url = Url.Combine(urlBase, path);

                var response = await url.GetAsync();
                var jsonString = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonConvert.DeserializeObject<HTTPResponse>(jsonString);

                return jsonObject;
            }
            catch (FlurlHttpException e)
            {
                return JsonConvert.DeserializeObject<HTTPResponse>("{ 'status': 'exception', 'message': '" + e.Message + "' }");
            }
        }

        private async Task<dynamic> Post(string urlBase, string path, string jsonString)
        {
            try
            {
                var url = Url.Combine(urlBase, path);

                var requestObject =  JsonConvert.DeserializeObject<object>(jsonString);
                var response = await url.PostJsonAsync(requestObject);
                
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonConvert.DeserializeObject(responseString);

                return jsonObject;
            }
            catch (FlurlHttpException e)
            {
                return JsonConvert.DeserializeObject("{ '__postchainerror__': true, 'message': '" + e.Message + "' }");
            }
            
        }

        private void ValidateMessageHash(string messageHash)
        {
            if (messageHash == null)
            {
                throw new Exception("messageHash is not a Buffer");
            }

            if (messageHash.Length != 64)
            {
                throw new Exception("expected length 64 of messageHash, but got " + messageHash.Length);
            }
        }
    }
}