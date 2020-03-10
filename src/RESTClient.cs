using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
#if UNITYBUILD
using UnityEngine.Networking;
#else
using Flurl;
using Flurl.Http;
#endif
using Newtonsoft.Json;

namespace Chromia.Postchain.Client
{
    internal class HTTPStatusResponse
    {
        public HTTPStatusResponse(string status, string message)
        {
            this.status = status;
            this.message = message;
        }

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
            
            return await Post<HTTPStatusResponse>(this.UrlBase, "tx/" + this.BlockchainRID, jsonString);
        }

        private async Task<HTTPStatusResponse> Status(string messageHash)
        {
            ValidateMessageHash(messageHash);
            return await Get(this.UrlBase, "tx/" + this.BlockchainRID + "/" + messageHash + "/status");
        }

        public async Task<object> Query<T>(string queryName, (string name, object content)[] queryObject)
        {
            var queryDict = QueryToDict(queryName, queryObject);
            string queryString = JsonConvert.SerializeObject(queryDict); // BuildQuery(queryObject);

            return await Post<T>(this.UrlBase, "query/" + this.BlockchainRID, queryString);
        }

        private Dictionary<string, object> QueryToDict(string queryName, (string name, object content)[] queryObject)
        {
            var queryDict = new Dictionary<string, object>();

            queryDict.Add("type", queryName);
            foreach (var entry in queryObject)
            {
                queryDict.Add(entry.name, entry.content);
            }

            return queryDict;
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

#if UNITYBUILD
        private async Task<HTTPStatusResponse> Get(string urlBase, string path)
        {
            var request = UnityWebRequest.Get(urlBase + path);

            await request.SendWebRequest();

            if (request.isNetworkError)
            {
                return new HTTPStatusResponse("exception", request.error);
            }
            else
            {
                return JsonConvert.DeserializeObject<HTTPStatusResponse>(request.downloadHandler.text);
            }
        }

        private async Task<object> Post<T>(string urlBase, string path, string jsonString)
        {
            var request = new UnityWebRequest(urlBase + path, "POST");            
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);

            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");  

            await request.SendWebRequest();

            if (request.isNetworkError)
            {
                return new HTTPStatusResponse("exception", request.error);
            }
            else
            {
                return JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
            }
        }
#else
        private async Task<HTTPStatusResponse> Get(string urlBase, string path)
        {
            try
            {
                var url = Url.Combine(urlBase, path);

                var response = await url.GetAsync();
                var jsonString = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<HTTPStatusResponse>(jsonString);
            }
            catch (FlurlHttpException e)
            {
                return new HTTPStatusResponse("exception", e.Message);
            }
        }

        private async Task<object> Post<T>(string urlBase, string path, string jsonString)
        {
            try
            {
                var url = Url.Combine(urlBase, path);

                var requestObject =  JsonConvert.DeserializeObject<object>(jsonString);
                var response = await url.PostJsonAsync(requestObject);
                
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonConvert.DeserializeObject<T>(responseString);

                return jsonObject;
            }
            catch (FlurlHttpException e)
            {
                return new HTTPStatusResponse("exception", e.Message);
            }
        }
#endif

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