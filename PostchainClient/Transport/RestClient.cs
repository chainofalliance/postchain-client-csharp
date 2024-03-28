using Chromia.Encoding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Chromia.TransactionReceipt;

namespace Chromia.Transport
{
    internal class RestClient
    {

        private enum Request
        {
            Get,
            PostBytes,
            PostJson
        }

        public Buffer BlockchainRID { get { return _blockchainRID; } }
        public int PollingRetries { get { return _pollingRetries; } }
        public int PollingInterval { get { return _pollingInterval; } }
        public int AttemptsPerEndpoint { get { return _attemptsPerEndpoint; } }
        public int AttemptInterval { get { return _attemptInterval; } }

        private readonly Buffer _blockchainRID;
        private readonly List<Uri> _nodeUrls;
        private int _pollingRetries = 10;
        private int _pollingInterval = 500;
        private int _attemptsPerEndpoint = 5;
        private int _attemptInterval = 500;

        private static ITransport _transport = new DefaultTransport();
        private static readonly Random _random = new Random();

        private Uri BaseUri => _nodeUrls[_random.Next(_nodeUrls.Count)];
        private Uri QueryUri => new Uri(BaseUri, $"./query_gtv/{_blockchainRID.Parse()}");
        private Uri TxUri => new Uri(BaseUri, $"./tx/{_blockchainRID.Parse()}");
        private Uri TxStatusUri(Buffer transactionRID) => new Uri(BaseUri, $"./tx/{_blockchainRID.Parse()}/{transactionRID.Parse()}/status");

        public RestClient(List<Uri> nodeUrl, Buffer blockchainRID) 
        {
            _nodeUrls = nodeUrl;
            _blockchainRID = blockchainRID;
        }

        #region Static
        public static void SetTransport(ITransport transport)
        {
            _transport = transport;
        }

        public async static Task<Buffer> GetBlockchainRID(Uri nodeUri, int blockchainIID, CancellationToken ct)
        {
            var uri = new Uri(nodeUri, "./brid/iid_" + blockchainIID);
            var response = await _transport.Get(uri, ct);
            return Buffer.From(response.ParseUTF8());
        }

        public static void EnsureBlockchainRID(Buffer blockchainRID)
        {
            if (blockchainRID.Length != 32)
                throw new ArgumentOutOfRangeException(nameof(blockchainRID), "has to be 32 bytes");
        }

        public async static Task<List<string>> GetNodesFromDirectory(List<Uri> directoryNodeUrls, Buffer blockchainRID, CancellationToken ct)
        {
            var directoryBrid = await GetBlockchainRID(directoryNodeUrls[0], 0, ct);
            var tmpClient = new RestClient(directoryNodeUrls, directoryBrid);
            var queryObject = new Dictionary<string, object>()
            {
                { "blockchain_rid", blockchainRID }
            };
            return await tmpClient.Query<List<string>>("cm_get_blockchain_api_urls", queryObject, ct);
        }

        #endregion

        #region Setter
        public void AddNodeUrl(Uri nodeUrl)
        {
            _nodeUrls.Add(nodeUrl);
        }

        public void AddNodeUrl(List<Uri> nodeUrl)
        {
            _nodeUrls.AddRange(nodeUrl);
        }

        public void SetPollingInterval(int pollingInterval)
        {
            _pollingInterval = pollingInterval;
        }

        public void SetPollingRetries(int pollingRetries)
        {
            _pollingRetries = pollingRetries;
        }

        public void SetAttemptsPerEndpoint(int attemptsPerEndpoint)
        {
            _attemptsPerEndpoint = attemptsPerEndpoint;
        }

        public void SetAttemptInterval(int attemptInterval)
        {
            _attemptInterval = attemptInterval;
        }
        #endregion

        public async Task<T> Query<T>(string name, object parameters, CancellationToken ct)
        {
            var queryObject = new object[] { name, parameters };
            var buffer = Gtv.Encode(queryObject);
            var response = await RequestWithRetries(Request.PostBytes, QueryUri, buffer, ct);
            var jsonObj = Gtv.Decode(response);
            if (jsonObj == null)
                return default;

            try
            {
                var jToken = JToken.FromObject(jsonObj);
                if (typeof(T) == typeof(float))
                    return (T)(object)float.Parse(jToken.ToObject<string>(), CultureInfo.InvariantCulture);
                else if (typeof(T) == typeof(double))
                    return (T)(object)double.Parse(jToken.ToObject<string>(), CultureInfo.InvariantCulture);
                else if (typeof(T) == typeof(Buffer))
                    return (T)jsonObj;
                else
                    return jToken.ToObject<T>();
            }
            catch (Exception e)
            {
                throw new ChromiaException($"failed to parse return data: {e.Message}");
            }
        }

        public Task<Buffer> SendTransaction(Transaction.Signed tx, CancellationToken ct)
        {
            var txObject = new JObject()
            {
                { "tx", tx.GtvBody.Parse() }
            };

            return RequestWithRetries(Request.PostJson, TxUri, JsonConvert.SerializeObject(txObject), ct);
        }

        public async Task<TransactionStatusResponse> GetTransactionStatus(Buffer transactionRID, CancellationToken ct)
        {
            var response = await RequestWithRetries(Request.Get, TxStatusUri(transactionRID), ct: ct);
            return JsonConvert.DeserializeObject<TransactionStatusResponse>(response.ParseUTF8());
        }

        public async Task<TransactionReceipt> WaitForConfirmation(Buffer transactionRID, int retry = 0, CancellationToken ct = default)
        {
            var txStatus = await GetTransactionStatus(transactionRID, ct);
            if (txStatus.Status == ResponseStatus.Waiting && retry < _pollingRetries)
            {
                await _transport.Delay(_pollingInterval, ct);
                return await WaitForConfirmation(transactionRID, retry+1, ct);
            }
            return new TransactionReceipt(transactionRID, txStatus, retry >= _pollingRetries);
        }

        private async Task<Buffer> RequestWithRetries(Request request, Uri uri, object content = null, CancellationToken ct = default)
        {
            var response = Buffer.Empty();
            var lastException = new TransportException(TransportException.ReasonCode.MalformedUri, "no nodes found");

            foreach (var endpoint in _nodeUrls.OrderBy(_ => _random.Next()))
            {
                for (var attempt = 0; attempt < _attemptsPerEndpoint; attempt++)
                {
                    try
                    { 
                        return request switch
                        {
                            Request.Get => await _transport.Get(uri, ct),
                            Request.PostBytes => await _transport.Post(uri, (Buffer)content, ct),
                            Request.PostJson => await _transport.Post(uri, (string)content, ct),
                            _ => throw new NotSupportedException($"request {request} not supported")
                        };
                    }
                    catch (TransportException e)
                    {
                        lastException = e;
                        await _transport.Delay(_attemptInterval, ct);
                    }
                }
            }

            throw lastException;
        }
    }
}
