﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chromia.Encoding;
using Chromia.Transport;
using static Chromia.Transport.RestClient;
using System.Security.Cryptography;
using System.Data.Common;

namespace Chromia
{
    /// <summary>
    /// Client to interact with the Chromia blockchain.
    /// </summary>
    public class ChromiaClient
    {
        /// <summary>
        /// The types this client supports as Operation and Query parameters.
        /// </summary>
        public static Type[] SupportedTypes => Gtv.SupportedTypes;

        /// <summary>
        /// The RID of the blockchain this client communicates with.
        /// </summary>
        public Buffer BlockchainRID { get { return _restClient.BlockchainRID; } }

        /// <summary>
        /// The amount of transaction status request retries before giving up.
        /// </summary>
        public int PollingRetries { get { return _restClient.PollingRetries; } }

        /// <summary>
        /// The interval between each transaction status poll.
        /// </summary>
        public int PollingInterval { get { return _restClient.PollingInterval; } }

        /// <summary>
        /// The amount of attempts to request at each endpoint.
        /// </summary>
        public int AttemptsPerEndpoint { get { return _restClient.AttemptsPerEndpoint; } }

        /// <summary>
        /// The interval between each request attempt.
        /// </summary>
        public int AttemptInterval { get { return _restClient.AttemptInterval; } }

        /// <summary>
        /// The urls of all nodes that are configured.
        /// </summary>
        public List<Uri> NodeUrls { get { return _restClient.NodeUrls; } }

        private readonly RestClient _restClient;

        private ChromiaClient(List<string> nodeUrls, Buffer blockchainRID)
            : this(nodeUrls?.ConvertAll(n => ToUri(n)), blockchainRID)
        {}

        private ChromiaClient(List<Uri> nodeUrls, Buffer blockchainRID)
        {
            EnsureBlockchainRID(blockchainRID);
            if (nodeUrls == null)
                throw new ArgumentNullException(nameof(nodeUrls));
            else if (nodeUrls.Count == 0)
                throw new ArgumentOutOfRangeException(nameof(nodeUrls));

            _restClient = new RestClient(nodeUrls, blockchainRID);
        }

        #region Static
        /// <summary>
        /// Sets the transport object to be used for interaction with the blockchain.
        /// Default is set to <see cref="DefaultTransport"/>.
        /// </summary>
        /// <param name="transport">The transport object to use for blockchain interaction.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void SetTransport(ITransport transport)
        {
            if (transport == null)
                throw new ArgumentNullException(nameof(transport));

            RestClient.SetTransport(transport);
        }

        /// <summary>
        /// Creates an sha256 hash of the given buffer.
        /// </summary>
        /// <param name="buffer">The buffer to hash.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <returns>The sha256 hash of the buffer.</returns>
        public static Buffer Sha256(Buffer buffer)
        {
            return Buffer.From(Sha256(buffer.Bytes));
        }

        /// <inheritdoc cref="Sha256(Buffer)"/>
        public static byte[] Sha256(byte[] buffer)
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(buffer);
        }

        /// <summary>
        /// Parses the object to gtv and creates the merkle root hash of it.
        /// </summary>
        /// <param name="obj">The object to hash.</param>
        /// <returns>The merkle root hash.</returns>
        public static Buffer Hash(object obj)
        {
            return Gtv.Hash(obj);
        }

        /// <summary>
        /// Encodes the given object to a gtv buffer.
        /// </summary>
        /// <param name="obj">The object to encode.</param>
        /// <returns>The gtv encoded object.</returns>
        /// <exception cref="ChromiaException"></exception>
        public static Buffer EncodeToGtv(object obj)
        {
            return Gtv.Encode(obj);
        }

        /// <summary>
        /// Decodes the given gtv buffer to an object.
        /// </summary>
        /// <param name="gtv">The gtv buffer to decode.</param>
        /// <returns>The decoded object.</returns>
        /// <exception cref="ChromiaException"></exception>
        public static object DecodeFromGtv(Buffer gtv)
        {
            return Gtv.Decode(gtv);
        }

        /// <summary>
        /// Creates a new <see cref="ChromiaClient"/> by getting the nodes from a directory.
        /// </summary>
        /// <param name="directoryNodeUrls">The directory nodes to query.</param>
        /// <param name="blockchainRID">The blockchain RID of the application.</param>
        /// <returns>This new <see cref="ChromiaClient"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="UriFormatException"></exception>
        public async static Task<ChromiaClient> CreateFromDirectory(List<string> directoryNodeUrls, Buffer blockchainRID)
        {
            EnsureBlockchainRID(blockchainRID);
            if (directoryNodeUrls == null)
                throw new ArgumentNullException(nameof(directoryNodeUrls));
            else if (directoryNodeUrls.Count == 0)
                throw new ArgumentOutOfRangeException(nameof(directoryNodeUrls));

            var convertedNodes = directoryNodeUrls.ConvertAll(n => ToUri(n));
            var nodes = await GetNodesFromDirectory(convertedNodes, blockchainRID);
            return new ChromiaClient(nodes, blockchainRID);
        }

        /// <inheritdoc cref="CreateFromDirectory(List{string}, Buffer)"/>
        /// <param name="directoryNodeUrl">The directory node to query.</param>
        /// <param name="blockchainRID">The blockchain RID of the application.</param>
        public async static Task<ChromiaClient> CreateFromDirectory(string directoryNodeUrl, Buffer blockchainRID)
        {
            return await CreateFromDirectory(new List<string>() { directoryNodeUrl }, blockchainRID);
        }

        /// <inheritdoc cref="CreateFromDirectory(List{string}, Buffer)"/>
        /// <param name="directoryNodeUrl">The directory node to query.</param>
        /// <param name="blockchainIID">The blockchain IID of the application. Gets resolved to the blockchain RID.</param>
        public async static Task<ChromiaClient> CreateFromDirectory(string directoryNodeUrl, int blockchainIID)
        {
            return await CreateFromDirectory(new List<string>() { directoryNodeUrl }, blockchainIID);
        }

        /// <inheritdoc cref="CreateFromDirectory(List{string}, Buffer)"/>
        /// <param name="directoryNodeUrls">The directory nodes to query.</param>
        /// <param name="blockchainIID">The blockchain IID of the application. Gets resolved to the blockchain RID.</param>
        public async static Task<ChromiaClient> CreateFromDirectory(List<string> directoryNodeUrls, int blockchainIID)
        {
            var blockchainRID = await GetBlockchainRID(directoryNodeUrls[0], blockchainIID);
            return await CreateFromDirectory(directoryNodeUrls, blockchainRID);
        }

        /// <summary>
        /// Creates a new <see cref="ChromiaClient"/> with the given nodes.
        /// </summary>
        /// <param name="nodeUrls">The nodes to interact with.</param>
        /// <param name="blockchainRID">The blockchain RID of the application.</param>
        /// <returns>This new <see cref="ChromiaClient"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="UriFormatException"></exception>
        public async static Task<ChromiaClient> Create(List<string> nodeUrls, Buffer blockchainRID)
        {
            await Task.FromResult(0);
            return new ChromiaClient(nodeUrls, blockchainRID);
        }

        /// <inheritdoc cref="Create(List{string}, Buffer)"/>
        public async static Task<ChromiaClient> Create(List<Uri> nodeUrls, Buffer blockchainRID)
        {
            await Task.FromResult(0);
            return new ChromiaClient(nodeUrls, blockchainRID);
        }

        /// <inheritdoc cref="Create(List{string}, Buffer)"/>
        /// <param name="nodeUrl">The node to interact with.</param>
        /// <param name="blockchainRID">The blockchain RID of the application.</param>
        public async static Task<ChromiaClient> Create(string nodeUrl, Buffer blockchainRID)
        {
            return await Create(new List<string>() { nodeUrl }, blockchainRID);
        }

        /// <inheritdoc cref="Create(string, Buffer)"/>
        public async static Task<ChromiaClient> Create(Uri nodeUrl, Buffer blockchainRID)
        {
            return await Create(new List<Uri>() { nodeUrl }, blockchainRID);
        }

        /// <inheritdoc cref="Create(List{string}, Buffer)"/>
        /// <param name="nodeUrl">The node to interact with.</param>
        /// <param name="blockchainIID">The blockchain IID of the application. Gets resolved to the blockchain RID.</param>
        public async static Task<ChromiaClient> Create(string nodeUrl, int blockchainIID)
        {
            return await Create(new List<string>() { nodeUrl }, blockchainIID);
        }

        /// <inheritdoc cref="Create(string, int)"/>
        public async static Task<ChromiaClient> Create(Uri nodeUrl, int blockchainIID)
        {
            return await Create(new List<Uri>() { nodeUrl }, blockchainIID);
        }

        /// <inheritdoc cref="Create(List{string}, Buffer)"/>
        /// <param name="nodeUrls">The nodes to interact with.</param>
        /// <param name="blockchainIID">The blockchain IID of the application. Gets resolved to the blockchain RID.</param>
        public async static Task<ChromiaClient> Create(List<string> nodeUrls, int blockchainIID)
        {
            return await Create(nodeUrls?.ConvertAll(n => ToUri(n)), blockchainIID);
        }

        /// <inheritdoc cref="Create(List{string}, int)"/>
        public async static Task<ChromiaClient> Create(List<Uri> nodeUrls, int blockchainIID)
        {
            var blockchainRID = await GetBlockchainRID(nodeUrls[0], blockchainIID);
            return await Create(nodeUrls, blockchainRID);
        }

        /// <summary>
        /// Resolves the blockchain RID for the give blockchain IID.
        /// </summary>
        /// <param name="nodeUrl">The node to interact with.</param>
        /// <param name="blockchainIID">The blockchain IID to resolve.</param>
        /// <returns>The blockchain RID of that blockchain</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="UriFormatException"></exception>
        public async static Task<Buffer> GetBlockchainRID(string nodeUrl, int blockchainIID)
        {
            if (nodeUrl == null) 
                throw new ArgumentNullException(nameof(nodeUrl));
            else if (blockchainIID < 0)
                throw new ArgumentOutOfRangeException(nameof(blockchainIID));

            return await GetBlockchainRID(ToUri(nodeUrl), blockchainIID);
        }

        /// <inheritdoc cref="GetBlockchainRID(string, int)"/>
        public async static Task<Buffer> GetBlockchainRID(Uri nodeUrl, int blockchainIID)
        {
            if (nodeUrl == null)
                throw new ArgumentNullException(nameof(nodeUrl));
            else if (blockchainIID < 0)
                throw new ArgumentOutOfRangeException(nameof(blockchainIID));

            return await RestClient.GetBlockchainRID(nodeUrl, blockchainIID);
        }

        private static Uri ToUri(string url)
        {
            if (!url.EndsWith("/"))
                url += "/";
            return new Uri(url);
        }
        #endregion

        #region Setter
        /// <summary>
        /// Adds a node to the node pool.
        /// </summary>
        /// <param name="nodeUrl">The node to add.</param>
        /// <returns>This object.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UriFormatException"></exception>
        public ChromiaClient AddNodeUrl(string nodeUrl)
        {
            if (nodeUrl == null)
                throw new ArgumentNullException(nameof(nodeUrl));

            _restClient.AddNodeUrl(ToUri(nodeUrl));
            return this;
        }

        /// <summary>
        /// Adds nodes to the node pool.
        /// </summary>
        /// <returns>This object.</returns>
        /// <param name="nodeUrl">The nodes to add.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UriFormatException"></exception>
        public ChromiaClient AddNodeUrl(List<string> nodeUrl)
        {
            nodeUrl.ForEach(n => AddNodeUrl(n));
            return this;
        }

        /// <summary>
        /// Sets the interval in which the status of a transaction is being polled
        /// while waiting for its confirmation.
        /// </summary>
        /// <returns>This object.</returns>
        /// <param name="pollingInterval">The interval in which the status gets polled.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ChromiaClient SetPollingInterval(int pollingInterval)
        {
            if (pollingInterval <= 0)
                throw new ArgumentOutOfRangeException(nameof(pollingInterval));

            _restClient.SetPollingInterval(pollingInterval);
            return this;
        }

        /// <summary>
        /// Sets the amount of retries the status of a transaction should be fetched for
        /// before giving up.
        /// </summary>
        /// <returns>This object.</returns>
        /// <param name="pollingRetries">The amount of tries to wait for transaction confirmation.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ChromiaClient SetPollingRetries(int pollingRetries)
        {
            if (pollingRetries <= 0)
                throw new ArgumentOutOfRangeException(nameof(pollingRetries));

            _restClient.SetPollingRetries(pollingRetries);
            return this;
        }

        /// <summary>
        /// The amount of attempts to send requests to each node before giving up.
        /// </summary>
        /// <returns>This object.</returns>
        /// <param name="attemptsPerEndpoint">The amount of retries per node.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ChromiaClient SetAttemptsPerEndpoint(int attemptsPerEndpoint)
        {
            if (attemptsPerEndpoint <= 0)
                throw new ArgumentOutOfRangeException(nameof(attemptsPerEndpoint));

            _restClient.SetAttemptsPerEndpoint(attemptsPerEndpoint);
            return this;
        }

        /// <summary>
        /// The interval between each retry while requesting the nodes.
        /// </summary>
        /// <returns>This object.</returns>
        /// <param name="attemptInterval">The interval between retries.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ChromiaClient SetAttemptInterval(int attemptInterval)
        {
            if (attemptInterval <= 0)
                throw new ArgumentOutOfRangeException(nameof(attemptInterval));

            _restClient.SetAttemptInterval(attemptInterval);
            return this;
        }
        #endregion

        /// <summary>
        /// Creates an empty <see cref="Transaction"/> object to fill.
        /// </summary>
        /// <returns>An empty transaction object.</returns>
        public Transaction TransactionBuilder()
        {
            return Transaction.Build(BlockchainRID);
        }

        /// <summary>
        /// Sends a unique transaction to the blockchain.
        /// A "no-operation" operation is added to the transaction.
        /// </summary>
        /// <param name="operation">The operation to send to the blockchain.</param>
        /// <param name="signer">The operation to send to the blockchain.</param>
        /// <returns>The transaction receipt of the transaction.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="TransportException"></exception>
        public async Task<TransactionReceipt> SendUniqueTransaction(Operation operation, SignatureProvider signer = null)
        {
            var tx = Transaction.Build(BlockchainRID)
                .AddOperation(operation);

            if (signer != null)
                tx.AddSignatureProvider(signer);

            return await SendUniqueTransaction(tx);
        }

        /// <inheritdoc cref="SendUniqueTransaction(Operation, SignatureProvider)"/>
        /// <param name="tx">The transaction to send to the blockchain.</param>
        public async Task<TransactionReceipt> SendUniqueTransaction(Transaction tx)
        {
            if (tx == null)
                throw new ArgumentNullException(nameof(tx));

            tx.AddNop();
            return await SendTransaction(tx);
        }

        /// <summary>
        /// Signs and sends a transaction to the blockchain.
        /// </summary>
        /// <param name="tx">The transatcion to send to the blockchain.</param>
        /// <returns>The transaction receipt of the transaction.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="TransportException"></exception>
        public async Task<TransactionReceipt> SendTransaction(Transaction tx)
        {
            if (tx == null)
                throw new ArgumentNullException(nameof(tx));

            var signedTx = tx.SetBlockchainRID(BlockchainRID).Sign();
            return await SendTransaction(signedTx);
        }

        /// <summary>
        /// Sends a signed transaction and waits for it to be processed.
        /// </summary>
        /// <param name="tx">The signed transaction to send.</param>
        /// <returns>A receipt for that transaction.</returns>
        /// <exception cref="TransportException"></exception>
        public async Task<TransactionReceipt> SendTransaction(Transaction.Signed tx)
        {
            if (tx.Signers.Count != tx.Signatures.Count)
                throw new InvalidOperationException($"unequal amount of signers and signatures");

            try
            {
                await _restClient.SendTransaction(tx);
            }
            catch (TransportException e)
            {
                if (e.StatusCode == 409)
                    return new TransactionReceipt(
                        tx.TransactionRID,
                        TransactionReceipt.ResponseStatus.DoubleTx,
                        "tx with hash already exists"
                    );
                else
                    throw e;
            }

            return await _restClient.WaitForConfirmation(tx.TransactionRID);
        }

        /// <summary>
        /// Gets the status of a transaction in the blockchain network.
        /// </summary>
        /// <param name="transactionRID">The transaction hash to check.</param>
        /// <returns>The current status of the transaction in the network.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="TransportException"></exception>
        public async Task<TransactionStatusResponse> GetTransactionStatus(Buffer transactionRID)
        {
            Transaction.EnsureRID(transactionRID);
            return await _restClient.GetTransactionStatus(transactionRID);
        }

        /// <summary>
        /// Queries data from the blockchain.
        /// </summary>
        /// <param name="name">The name of the query.</param>
        /// <param name="parameters">The parameters of the query.</param>
        /// <returns>The data parsed as the given type.</returns>
        /// <exception cref="ChromiaException"></exception>
        /// <exception cref="TransportException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private async Task<T> Query<T>(string name, object parameters)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            else if (name.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(name));

            return await _restClient.Query<T>(name, parameters);
        }

        /// <inheritdoc cref="Query{T}(string, object)"/>
        /// <param name="name">The name of the query.</param>
        /// <param name="obj">A gtv serializable object as query parameters.</param>
        public async Task<T> Query<T>(string name, IGtvSerializable obj)
        {
            var jsonObj = JObject.FromObject(obj);
            return await Query<T>(name, jsonObj);
        }

        /// <inheritdoc cref="Query{T}(string, object)"/>
        public async Task<T> Query<T>(string name, params (string name, object content)[] parameters)
        {
            foreach (var item in parameters)
                if (!Gtv.IsOfValidType(item.content))
                    throw new ArgumentException(nameof(parameters), "unsupported data type for " + item.content);

            var parameterDict = parameters.ToDictionary(p => p.name, p => p.content);
            return await Query<T>(name, parameterDict);
        }

        /// <summary>
        /// Retrieves a confirmation proof for a transaction with the specified sha256
        /// hash.
        /// <para>
        /// If first parameter is null, then the second parameter is an object
        /// like the following:
        /// <code>
        /// {
        ///     hash: messageHashBuffer,
        ///     blockHeader: blockHeaderBuffer,
        ///     signatures: [
        ///                     {pubKey: pubKeyBuffer, signature: sigBuffer},
        ///                     ...
        ///                 ],
        ///     merklePath: [
        ///                     {side: &lt;0|1&gt;, hash: &lt;hash buffer level n-1&gt;},
        ///                     ...
        ///                     {side: &lt;0|1&gt;, hash: &lt; hash buffer level 1 &gt;}
        ///                 ]
        /// }
        /// </code>
        ///
        /// If no such transaction RID exists, the callback will be called with (null, null).
        /// The proof object can be validated using
        /// postchain - common.util.validateMerklePath(proof.merklePath, proof.hash,
        /// proof.blockHeader.slice(32, 64))
        ///
        /// The signatures must be validated agains some know trusted source for valid signers
        /// at this specific block height.
        /// </para>
        /// </summary>
        /// <param name="transactionRID"></param>
        /// <returns></returns>
        //public async Task<Buffer> GetConfirmationProof(Buffer transactionRID)
        //{
        //    return await Query<Buffer>("cm_get_blockchain_cluster", ("brid", blockchainRID));
        //}

        //public static async Transaction CreateIccfProofTransaction(
        //    ChromiaClient client,
        //    Buffer txToProveRid,
        //    Buffer txToProveHash,
        //    List<Buffer> txToProveSigners,
        //    Buffer sourceBlockchainRID,
        //    Buffer targetBlockchainRID,
        //    List<Buffer> iccfTxSigners,
        //    bool forceIntraNetworkIccfOperation = false
        //)
        //{
        //    var sourceClient = await Create(client.NodeUrls, sourceBlockchainRID);
        //    var txProof = sourceClient.GetConfirmationProof(txToProveRid);

        //    if (txProof == null)
        //        throw new ChromiaException("tx not found in source blockchain");

        //    if (txProof.Hash != txToProveHash)
        //        throw new ChromiaException("tx hash mismatch");

        //    var sourceCluster = await client.Query<Buffer>("cm_get_blockchain_cluster", ("brid", sourceBlockchainRID));
        //    var targetCluster = await client.Query<Buffer>("cm_get_blockchain_cluster", ("brid", targetBlockchainRID));

        //    if (!forceIntraNetworkIccfOperation && sourceCluster == targetCluster)
        //    {
                
        //    }
        //}
    }
}
