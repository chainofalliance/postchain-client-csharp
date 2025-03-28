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
using System.Threading;
using Newtonsoft.Json;

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
        /// The version of the hash algorithm to use for transactions on this blockchain.
        /// </summary>
        public int HashVersion { get; }

        private readonly RestClient _restClient;

        private ChromiaClient(List<Uri> nodeUrls, Buffer blockchainRID, int hashVersion)
        {
            EnsureBlockchainRID(blockchainRID);
            if (nodeUrls == null)
                throw new ArgumentNullException(nameof(nodeUrls));
            else if (nodeUrls.Count == 0)
                throw new ArgumentOutOfRangeException(nameof(nodeUrls));

            _restClient = new RestClient(nodeUrls, blockchainRID);
            HashVersion = hashVersion;
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
        /// The hash version depends on the configuration of the blockchain in <c>chromia.yml</c>.
        /// Refer to the documentation for more information.
        /// https://docs.chromia.com/intro/configuration/project-config <br />
        /// Version 1 is the legacy hash algorithm. Will be deprecated in the future. <br />
        /// Version 2 is the new hash algorithm that fixes some edge cases. <br />
        /// </summary>
        /// <param name="obj">The object to hash.</param>
        /// <param name="hashVersion">The hash version to use.</param>
        /// <returns>The merkle root hash.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Buffer Hash(object obj, int hashVersion)
        {
            if (hashVersion <= 0 || hashVersion > 2)
                throw new ArgumentOutOfRangeException(nameof(hashVersion));

            return Gtv.Hash(obj, hashVersion);
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
        /// <param name="ct">A cancellation token to abort the task.</param>
        /// <returns>This new <see cref="ChromiaClient"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="UriFormatException"></exception>
        public async static Task<ChromiaClient> CreateFromDirectory(List<string> directoryNodeUrls, Buffer blockchainRID, CancellationToken ct = default)
        {
            EnsureBlockchainRID(blockchainRID);
            if (directoryNodeUrls == null)
                throw new ArgumentNullException(nameof(directoryNodeUrls));
            else if (directoryNodeUrls.Count == 0)
                throw new ArgumentOutOfRangeException(nameof(directoryNodeUrls));

            var convertedNodes = directoryNodeUrls.Select(ToUri).ToList();
            var nodes = await GetNodesFromDirectory(convertedNodes, blockchainRID, ct);
            var hashVersion = await GetHashVersion(nodes, blockchainRID, ct);
            return new ChromiaClient(nodes, blockchainRID, hashVersion);
        }

        /// <inheritdoc cref="CreateFromDirectory(List{string}, Buffer, CancellationToken)"/>
        /// <param name="directoryNodeUrl">The directory node to query.</param>
        /// <param name="blockchainRID">The blockchain RID of the application.</param>
        /// <param name="ct">A cancellation token to abort the task.</param>
        public static Task<ChromiaClient> CreateFromDirectory(string directoryNodeUrl, Buffer blockchainRID, CancellationToken ct = default)
        {
            return CreateFromDirectory(new List<string>() { directoryNodeUrl }, blockchainRID, ct);
        }

        /// <inheritdoc cref="CreateFromDirectory(List{string}, Buffer, CancellationToken)"/>
        /// <param name="directoryNodeUrl">The directory node to query.</param>
        /// <param name="blockchainIID">The blockchain IID of the application. Gets resolved to the blockchain RID.</param>
        /// <param name="ct">A cancellation token to abort the task.</param>
        public static Task<ChromiaClient> CreateFromDirectory(string directoryNodeUrl, int blockchainIID, CancellationToken ct = default)
        {
            return CreateFromDirectory(new List<string>() { directoryNodeUrl }, blockchainIID, ct);
        }

        /// <inheritdoc cref="CreateFromDirectory(List{string}, Buffer, CancellationToken)"/>
        /// <param name="directoryNodeUrls">The directory nodes to query.</param>
        /// <param name="blockchainIID">The blockchain IID of the application. Gets resolved to the blockchain RID.</param>
        /// <param name="ct">A cancellation token to abort the task.</param>
        public async static Task<ChromiaClient> CreateFromDirectory(List<string> directoryNodeUrls, int blockchainIID, CancellationToken ct = default)
        {
            var blockchainRID = await GetBlockchainRID(directoryNodeUrls[0], blockchainIID, ct);
            return await CreateFromDirectory(directoryNodeUrls, blockchainRID, ct);
        }

        /// <summary>
        /// Creates a new <see cref="ChromiaClient"/> with the given nodes.
        /// </summary>
        /// <param name="nodeUrls">The nodes to interact with.</param>
        /// <param name="blockchainRID">The blockchain RID of the application.</param>
        /// <param name="ct">A cancellation token to abort the task.</param>
        /// <returns>This new <see cref="ChromiaClient"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="UriFormatException"></exception>
        public static async Task<ChromiaClient> Create(List<string> nodeUrls, Buffer blockchainRID, CancellationToken ct = default)
        {
            var convertedNodes = nodeUrls.Select(ToUri).ToList();
            var hashVersion = await GetHashVersion(convertedNodes, blockchainRID, ct);
            return new ChromiaClient(convertedNodes, blockchainRID, hashVersion);
        }

        /// <inheritdoc cref="Create(List{string}, Buffer, CancellationToken)"/>
        /// <param name="nodeUrl">The node to interact with.</param>
        /// <param name="blockchainRID">The blockchain RID of the application.</param>
        /// <param name="ct">A cancellation token to abort the task.</param>
        public async static Task<ChromiaClient> Create(string nodeUrl, Buffer blockchainRID, CancellationToken ct = default)
        {
            return await Create(new List<string>() { nodeUrl }, blockchainRID, ct);
        }

        /// <inheritdoc cref="Create(List{string}, Buffer, CancellationToken)"/>
        /// <param name="nodeUrl">The node to interact with.</param>
        /// <param name="blockchainIID">The blockchain IID of the application. Gets resolved to the blockchain RID.</param>
        /// <param name="ct">A cancellation token to abort the task.</param>
        public async static Task<ChromiaClient> Create(string nodeUrl, int blockchainIID, CancellationToken ct = default)
        {
            return await Create(new List<string>() { nodeUrl }, blockchainIID, ct);
        }

        /// <inheritdoc cref="Create(List{string}, Buffer, CancellationToken)"/>
        /// <param name="nodeUrls">The nodes to interact with.</param>
        /// <param name="blockchainIID">The blockchain IID of the application. Gets resolved to the blockchain RID.</param>
        /// <param name="ct">A cancellation token to abort the task.</param>
        public async static Task<ChromiaClient> Create(List<string> nodeUrls, int blockchainIID, CancellationToken ct = default)
        {
            var blockchainRID = await GetBlockchainRID(nodeUrls[0], blockchainIID, ct);
            return await Create(nodeUrls, blockchainRID, ct);
        }

        /// <summary>
        /// Resolves the blockchain RID for the give blockchain IID.
        /// </summary>
        /// <param name="nodeUrl">The node to interact with.</param>
        /// <param name="blockchainIID">The blockchain IID to resolve.</param>
        /// <param name="ct">A cancellation token to abort the task.</param>
        /// <returns>The blockchain RID of that blockchain</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="UriFormatException"></exception>
        public static Task<Buffer> GetBlockchainRID(string nodeUrl, int blockchainIID, CancellationToken ct = default)
        {
            if (nodeUrl == null)
                throw new ArgumentNullException(nameof(nodeUrl));
            else if (blockchainIID < 0)
                throw new ArgumentOutOfRangeException(nameof(blockchainIID));

            return RestClient.GetBlockchainRID(ToUri(nodeUrl), blockchainIID, ct);
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
            return Transaction.Build(BlockchainRID, HashVersion);
        }

        /// <summary>
        /// Sends a unique transaction to the blockchain.
        /// A "no-operation" operation is added to the transaction.
        /// </summary>
        /// <param name="operation">The operation to send to the blockchain.</param>
        /// <param name="signer">The operation to send to the blockchain.</param>
        /// <param name="ct">A cancellation token to abort the task.</param>
        /// <returns>The transaction receipt of the transaction.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="TransportException"></exception>
        public Task<TransactionReceipt> SendUniqueTransaction(
            Operation operation,
            ISignatureProvider signer = null,
            CancellationToken ct = default
        )
        {
            var tx = Transaction.Build(BlockchainRID, HashVersion)
                .AddOperation(operation);

            if (signer != null)
                tx.AddSignatureProvider(signer);

            return SendUniqueTransaction(tx, ct);
        }

        /// <inheritdoc cref="SendUniqueTransaction(Operation, ISignatureProvider, CancellationToken)"/>
        /// <param name="tx">The transaction to send to the blockchain.</param>
        /// <param name="ct">A cancellation token to abort the task.</param>
        public Task<TransactionReceipt> SendUniqueTransaction(Transaction tx, CancellationToken ct = default)
        {
            if (tx == null)
                throw new ArgumentNullException(nameof(tx));

            tx.AddNop();
            return SendTransaction(tx, ct);
        }

        /// <summary>
        /// Signs and sends a transaction to the blockchain.
        /// </summary>
        /// <param name="tx">The transatcion to send to the blockchain.</param>
        /// <param name="ct">A cancellation token to abort the task.</param>
        /// <returns>The transaction receipt of the transaction.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="TransportException"></exception>
        public async Task<TransactionReceipt> SendTransaction(Transaction tx, CancellationToken ct = default)
        {
            if (tx == null)
                throw new ArgumentNullException(nameof(tx));

            tx.UseHashVersion(HashVersion);
            var signedTx = tx.SetBlockchainRID(BlockchainRID).Sign();
            return await SendTransaction(signedTx, ct);
        }

        /// <summary>
        /// Sends a signed transaction and waits for it to be processed.
        /// </summary>
        /// <param name="tx">The signed transaction to send.</param>
        /// <param name="ct">A cancellation token to abort the task.</param>
        /// <returns>A receipt for that transaction.</returns>
        /// <exception cref="TransportException"></exception>
        public async Task<TransactionReceipt> SendTransaction(Transaction.Signed tx, CancellationToken ct = default)
        {
            if (tx.Signers.Count != tx.Signatures.Count)
                throw new InvalidOperationException($"unequal amount of signers and signatures");

            try
            {
                await _restClient.SendTransaction(tx, ct);
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
                    throw;
            }

            return await _restClient.WaitForConfirmation(tx.TransactionRID, ct: ct);
        }

        /// <summary>
        /// Gets the status of a transaction in the blockchain network.
        /// </summary>
        /// <param name="transactionRID">The transaction hash to check.</param>
        /// <param name="ct">A cancellation token to abort the task.</param>
        /// <returns>The current status of the transaction in the network.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="TransportException"></exception>
        public Task<TransactionStatusResponse> GetTransactionStatus(Buffer transactionRID, CancellationToken ct = default)
        {
            Transaction.EnsureRID(transactionRID);
            return _restClient.GetTransactionStatus(transactionRID, ct);
        }

        /// <summary>
        /// Queries data from the blockchain.
        /// </summary>
        /// <param name="name">The name of the query.</param>
        /// <param name="parameters">The parameters of the query.</param>
        /// <param name="ct">A cancellation token to abort the task.</param>
        /// <returns>The data parsed as the given type.</returns>
        /// <exception cref="ChromiaException"></exception>
        /// <exception cref="TransportException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Task<T> Query<T>(string name, object parameters, CancellationToken ct = default)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            else if (name.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(name));

            return _restClient.Query<T>(name, parameters, ct);
        }

        /// <inheritdoc cref="Query{T}(string, object, CancellationToken)"/>
        public Task<T> Query<T>(string name, params (string name, object content)[] parameters)
        {
            return Query<T>(name, CancellationToken.None, parameters);
        }

        /// <inheritdoc cref="Query{T}(string, object, CancellationToken)"/>
        public Task<T> Query<T>(string name, CancellationToken ct = default, params (string name, object content)[] parameters)
        {
            foreach (var item in parameters)
                if (!Gtv.IsOfValidType(item.content))
                    throw new ArgumentException("unsupported data type for " + item.content, nameof(parameters));

            var parameterDict = parameters.ToDictionary(p => p.name, p => p.content);
            return Query<T>(name, parameterDict, ct);
        }
    }
}
