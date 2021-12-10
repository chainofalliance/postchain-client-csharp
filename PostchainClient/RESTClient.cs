using System;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

using System.IO;
using System.Net.Http;
using System.Text;

namespace Chromia.Postchain.Client
{
    internal class HTTPStatusResponse
    {
        public HTTPStatusResponse(string status, string rejectReason)
        {
            this.status = status;
            this.rejectReason = rejectReason;
        }

        public string status = "";
        public string rejectReason = "";
    }

    public class RESTClient
    {
        public string BlockchainRID { get; private set; }
        public int RequestTimeout
        {
            get => _requestTimout;
            set
            {
                if (value >= 0 || value == System.Threading.Timeout.Infinite)
                {
                    _requestTimout = value;
                }
            }
        }

        private string _urlBase;
        private int _requestTimout = 1000;
        private HttpClient _httpClient;

        ///<summary>
        ///Create new RESTClient object.
        ///</summary>
        ///<param name = "urlBase">URL to rest server.</param>
        ///<param name = "blockchainRID">RID of blockchain.</param>
        public RESTClient(string urlBase, string blockchainRID = null)
        {
            BlockchainRID = blockchainRID;
            _urlBase = urlBase;
            _httpClient = new HttpClient();
        }

        public async Task<PostchainErrorControl> InitializeBRIDFromChainID(int chainID)
        {
            var brid = await Get<string>(this._urlBase, "brid/iid_" + chainID, true);
            if (brid is HTTPStatusResponse)
            {
                return new PostchainErrorControl(true, ((HTTPStatusResponse)brid).rejectReason);
            }
            else if (brid is string)
            {
                this.BlockchainRID = (string)brid;

                return new PostchainErrorControl();
            }
            else
            {
                return new PostchainErrorControl(true, "Unknown query return type " + brid.GetType().ToString());
            }
        }

        internal async Task<object> PostTransaction(string serializedTransaction)
        {
            string jsonString = String.Format(@"{{""tx"": ""{0}""}}", serializedTransaction);

            return await Post<HTTPStatusResponse>(this._urlBase, "tx/" + this.BlockchainRID, jsonString);
        }

        internal async Task<PostchainErrorControl> PostAndWaitConfirmation(string serializedTransaction, string txRID)
        {
            await this.PostTransaction(serializedTransaction);

            return await this.WaitConfirmation(txRID);
        }

        internal async Task<object> Query<T>(string queryName, (string name, object content)[] queryObject)
        {
            var queryDict = QueryToDict(queryName, queryObject);
            string queryString = JsonConvert.SerializeObject(queryDict);

            return await Post<T>(this._urlBase, "query/" + this.BlockchainRID, queryString);
        }

        private async Task<HTTPStatusResponse> Status(string messageHash)
        {
            ValidateMessageHash(messageHash);
            return (HTTPStatusResponse)await Get<HTTPStatusResponse>(this._urlBase, "tx/" + this.BlockchainRID + "/" + messageHash + "/status");
        }

        private Dictionary<string, object> QueryToDict(string queryName, (string name, object content)[] queryObject)
        {
            var queryDict = new Dictionary<string, object>();

            queryDict.Add("type", queryName);
            foreach (var entry in queryObject)
            {
                if (entry.content is byte[])
                {
                    queryDict.Add(entry.name, PostchainUtil.ByteArrayToString((byte[])entry.content));
                }
                else
                {
                    queryDict.Add(entry.name, entry.content);
                }
            }

            return queryDict;
        }

        private static bool IsTuple(Type tuple)
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

        private async Task<PostchainErrorControl> WaitConfirmation(string txRID)
        {
            var response = await this.Status(txRID);

            switch (response.status)
            {
                case "confirmed":
                    return new PostchainErrorControl(false, "");
                case "rejected":
                case "unknown":
                    return new PostchainErrorControl(true, response.rejectReason);
                case "waiting":
                    await Task.Delay(511);
                    return await this.WaitConfirmation(txRID);
                case "exception":
                    return new PostchainErrorControl(true, "HTTP Exception: " + response.rejectReason);
                default:
                    return new PostchainErrorControl(true, "Got unexpected response from server: " + response.status);
            }
        }

        private async Task<object> ParseResponse<T>(HttpResponseMessage response, bool raw = false)
        {
            response.EnsureSuccessStatusCode();
            
            var contentStream = await response.Content.ReadAsStreamAsync();

            using (var streamReader = new StreamReader(contentStream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var responseText = await streamReader.ReadToEndAsync();

                if (raw) return responseText;

                return JsonConvert.DeserializeObject<T>(responseText);
            }
        }

        private async Task<object> Get<T>(string urlBase, string path, bool raw = false)
        {
            try
            {
                var response = await _httpClient.GetAsync(urlBase + path);
                return await ParseResponse<T>(response, raw);
            }
            catch (Exception e)
            {
                return new HTTPStatusResponse("exception", e.Message);
            }
        }

        private async Task<object> Post<T>(string urlBase, string path, string jsonString)
        {
            try
            {
                var response = await _httpClient.PostAsync(urlBase + path, new StringContent(jsonString, Encoding.UTF8, "application/json"));
                return await ParseResponse<T>(response);
            }
            catch (Exception e)
            {
                return new HTTPStatusResponse("exception", e.Message);
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