using System;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;

namespace Chromia.PostchainClient
{
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

        public async Task<dynamic> PostTransaction(string serializedTransaction)
        {
            string jsonString = String.Format(@"{{""tx"": ""{0}""}}", serializedTransaction);
            
            return await Post(this.UrlBase, "tx/" + this.BlockchainRID, jsonString);
        }

        private async Task<dynamic> Status(string messageHash)
        {
            ValidateMessageHash(messageHash);

            return await Get(this.UrlBase, "tx/" + this.BlockchainRID + "/" + messageHash + "/status");
        }

        public async Task<dynamic> Query(string queryName, params dynamic[] queryObject)
        {
            string queryString = BuildQuery(queryName, queryObject);

            return await Post(this.UrlBase, "query/" + this.BlockchainRID, queryString);
        }

        private string BuildQuery(string queryName, dynamic[] queryObject)
        {
            string queryString = String.Format(@"{{""type"": ""{0}"",", queryName);

            foreach (dynamic queryParam in queryObject)
            {
                if (queryParam.Item2 is System.Array)
                {
                    queryString += String.Format(@"""{0}"": ""{1}"",", queryParam.Item1, Util.ByteArrayToString(queryParam.Item2));
                }
                else if (queryParam.Item2 is System.Int32)
                {
                    queryString += String.Format(@"""{0}"": {1},", queryParam.Item1, queryParam.Item2);
                }
                else
                {
                    queryString += String.Format(@"""{0}"": ""{1}"",", queryParam.Item1, queryParam.Item2);
                }
            }

            queryString = queryString.Remove(queryString.Length - 1) + "}";

            return queryString;
        }

        public async Task<dynamic> WaitConfirmation(string txRID)
        {
            var status = await this.Status(txRID);

            var statusString = status.status.ToObject<string>();
            switch(statusString)
            {
                case "confirmed":
                    return null;
                case "rejected":
                    return "Message was rejected";
                case "unknown":                                
                    return "Server lost our message";
                case "waiting":
                    await Task.Delay(511);
                    return await this.WaitConfirmation(txRID);
                default:
                    return "Got unexpected response from server: " + statusString;
            }
        }

        public async Task<dynamic> PostAndWaitConfirmation(string serializedTransaction, string txRID)
        {
            await this.PostTransaction(serializedTransaction);

            return await this.WaitConfirmation(txRID);
        }

        private async Task<dynamic> Get(string urlBase, string path)
        {
            try
            {
                var url = Url.Combine(urlBase, path);

                var response = await url.GetAsync();
                var jsonObject = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());

                return jsonObject;
            }
            catch (FlurlHttpException e)
            {
                return e;
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
                return e;
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

        private string StringToHex(string stringValue)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in stringValue)
            { 
                sb.Append(Convert.ToInt32(c).ToString("X")); 
            }
            return sb.ToString();
        }

        [Obsolete]
        public async Task<dynamic> GetTransaction(string messageHash, Action<string, dynamic> callback)
        {
            ValidateMessageHash(messageHash);

            return await Get(this.UrlBase, "tx/" + this.BlockchainRID + "/" + messageHash);
        }

        [Obsolete]
        public async Task<dynamic> GetConfirmationProof(string messageHash, Action<string, string> callback)
        {
            ValidateMessageHash(messageHash);

            return await Get(UrlBase, "tx/" + this.BlockchainRID + "/" + messageHash + "/confirmationProof");
        }

        [Obsolete]
        private void HandleGetResponse(string error, int statusCode, string responseObject, Action<string, dynamic> callback)
        {
            if (error == "")
            {
                callback(error, null);
            } else if (statusCode == 404)
            {
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

        [Obsolete]
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