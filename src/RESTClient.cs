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
        private string BlockhainRID;

        public RESTClient(string urlBase, string blockhainRID)
        {
            this.UrlBase = urlBase;
            this.BlockhainRID = blockhainRID;
        }

        public async Task<dynamic> PostTransaction(string serializedTransaction)
        {
            string jsonString = String.Format(@"{{""tx"": ""{0}""}}", serializedTransaction);
            
            return await Post(this.UrlBase, "tx/" + this.BlockhainRID, jsonString);
        }

        public async Task<dynamic> Status(string messageHash)
        {
            ValidateMessageHash(messageHash);

            return await Get(this.UrlBase, "tx/" + this.BlockhainRID + "/" + messageHash + "/status");
        }

        public async Task<dynamic> Query(string queryName, dynamic queryObject)
        {
            queryObject.Add(("type", queryName));

            string queryString = "{";

            foreach (dynamic queryParam in queryObject)
            {
                queryString += String.Format(@"""{0}"": ""{1}"",", queryParam.Item1, queryParam.Item2);
            }

            queryString = queryString.Remove(queryString.Length - 1) + "}";

            return await Post(this.UrlBase, "query/" + this.BlockhainRID, queryString);
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

            return await Get(this.UrlBase, "tx/" + this.BlockhainRID + "/" + messageHash);
        }

        [Obsolete]
        public async Task<dynamic> GetConfirmationProof(string messageHash, Action<string, string> callback)
        {
            ValidateMessageHash(messageHash);

            return await Get(UrlBase, "tx/" + this.BlockhainRID + "/" + messageHash + "/confirmationProof");
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