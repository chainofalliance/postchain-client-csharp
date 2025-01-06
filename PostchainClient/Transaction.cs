using Chromia.Encoding;
using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using static Chromia.TransactionReceipt;
using static Chromia.Transport.RestClient;

namespace Chromia
{
    /// <summary>
    /// Status of a transaction in the blockchain network.
    /// </summary>
    public readonly struct TransactionStatusResponse
    {
        /// <summary>
        /// Response status code from the network.
        /// </summary>
        [JsonProperty("status")]
        public readonly ResponseStatus Status;

        /// <summary>
        /// Reason why the transaction failed should <see cref="Status"/> be <see cref="ResponseStatus.Rejected"/>.
        /// </summary>
        [JsonProperty("rejectReason")]
        public readonly string RejectReason;
    }

    /// <summary>
    /// Receipt for a transaction containing its status.
    /// </summary>
    public readonly struct TransactionReceipt
    {
        /// <summary>
        /// Response status code from the network.
        /// </summary>
        public enum ResponseStatus
        {
            /// <summary>
            /// Transaction was confirmed.
            /// </summary>
            [EnumMember(Value = "confirmed")]
            Confirmed,
            /// <summary>
            /// Transaction was rejected. Check <see cref="RejectReason"/> why.
            /// </summary>
            [EnumMember(Value = "rejected")]
            Rejected,
            /// <summary>
            /// Transaction with that transaction RID doesn't exist in network.
            /// </summary>
            [EnumMember(Value = "unknown")]
            Unknown,
            /// <summary>
            /// Transaction is waiting to be confirmed by network.
            /// </summary>
            [EnumMember(Value = "waiting")]
            Waiting,
            /// <summary>
            /// Waiting for the confirmation of a transaction timed out.
            /// </summary>
            [EnumMember(Value = "timeout")]
            Timeout,
            /// <summary>
            /// The transaction could not be added because a transaction with the
            /// transaction RID already exists in network.
            /// </summary>
            [EnumMember(Value = "double_tx")]
            DoubleTx
        }

        /// <summary>
        /// The last known status of the transaction in the network.
        /// </summary>
        public readonly ResponseStatus Status;

        /// <summary>
        /// Contains the reason why the transaction was rejected in case 
        /// <see cref="Status"/> is set to <see cref="ResponseStatus.Rejected"/>.
        /// Empty otherwise.
        /// </summary>
        public readonly string RejectReason;

        /// <summary>
        /// The transaction hash.
        /// </summary>
        public readonly Buffer TransactionRID;


        internal TransactionReceipt(Buffer transactionRID, TransactionStatusResponse response, bool isTimeout)
        {
            Status = isTimeout ? ResponseStatus.Timeout : response.Status;
            RejectReason = response.RejectReason;
            TransactionRID = transactionRID;
        }

        internal TransactionReceipt(Buffer transactionRID, ResponseStatus status, string message)
        {
            Status = status;
            RejectReason = message;
            TransactionRID = transactionRID;
        }
    }

    /// <summary>
    /// Contains information about a Chromia transaction.
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// The transaction hash.
        /// </summary>
        public Buffer TransactionRID() => Gtv.Hash(GetBody());

        private Buffer _blockchainRID;
        private readonly List<Operation> _operations;
        private readonly HashSet<Buffer> _signers;
        private readonly HashSet<ISignatureProvider> _signatureProviders;


        /// <summary>
        /// Builds a new empty transaction.
        /// </summary>
        /// <returns>The empty <see cref="Transaction"/>.</returns>
        public static Transaction Build()
        {
            return Build(Buffer.Empty(), null, null, null);
        }

        /// <summary>
        /// Builds a new empty transaction.
        /// </summary>
        /// <param name="blockchainRID">The RID of the blockchain.</param>
        /// <returns>The empty <see cref="Transaction"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Transaction Build(Buffer blockchainRID)
        {
            return Build(blockchainRID, null, null, null);
        }

        /// <summary>
        /// Builds a new transaction.
        /// </summary>
        /// <param name="blockchainRID">The RID of the blockchain.</param>
        /// <param name="operations">The operations to be added to the transaction.</param>
        /// <param name="signers">The signers that need to sign the transaction.</param>
        /// <param name="signatureProviders">The <see cref="ISignatureProvider"/> that sign the transaction.</param>
        /// <returns>The new <see cref="Transaction"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Transaction Build(
            Buffer blockchainRID,
            List<Operation> operations,
            HashSet<Buffer> signers,
            HashSet<ISignatureProvider> signatureProviders
        )
        {
            operations ??= new List<Operation>();
            signers ??= new HashSet<Buffer>();
            signatureProviders ??= new HashSet<ISignatureProvider>();

            return new Transaction(blockchainRID, operations, signers, signatureProviders);
        }

        /// <summary>
        /// Decodes a buffer to a signed transaction.
        /// </summary>
        /// <param name="buffer">The buffer to decode.</param>
        /// <returns>The decoded <see cref="Signed"/> object.</returns>
        /// <exception cref="ChromiaException"></exception>
        public static Signed Decode(Buffer buffer)
        {
            return Signed.From(buffer);
        }

        /// <summary>
        /// Ensures a buffer is a valid transaction RID. Throws an exception if not.
        /// </summary>
        /// <param name="transactionRID">The transaction RID to check.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void EnsureRID(Buffer transactionRID)
        {
            if (transactionRID.Length != 64)
                throw new ArgumentOutOfRangeException(nameof(transactionRID), "must be 64 bytes");
        }

        private Transaction(
            Buffer blockchainRID,
            List<Operation> operations,
            HashSet<Buffer> signers,
            HashSet<ISignatureProvider> signatureProviders
        )
        {
            if (!blockchainRID.IsEmpty && blockchainRID.Length != 32)
                throw new ArgumentOutOfRangeException(nameof(blockchainRID), "has to be empty or 32 bytes");

            _blockchainRID = blockchainRID;
            _operations = operations;
            _signers = signers;
            _signatureProviders = signatureProviders;
        }

        /// <summary>
        /// Sets the blockchain RID of the transaction.
        /// </summary>
        /// <param name="blockchainRID">The RID of the blockchain to set.</param>
        /// <returns>This object.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Transaction SetBlockchainRID(Buffer blockchainRID)
        {
            EnsureBlockchainRID(blockchainRID);

            _blockchainRID = blockchainRID;
            return this;
        }

        /// <summary>
        /// Adds an <see cref="Operation"/> to the transaction.
        /// </summary>
        /// <param name="operation">The operation to add to the transaction.</param>
        /// <returns>This object.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Transaction AddOperation(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            _operations.Add(operation);
            return this;
        }

        /// <summary>
        /// Adds multiple <see cref="Operation"/> to the transaction.
        /// </summary>
        /// <param name="operations">The operations to add to the transaction.</param>
        /// <returns>This object.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Transaction AddOperations(IEnumerable<Operation> operations)
        {
            if (operations == null)
                throw new ArgumentNullException(nameof(operations));

            operations.ToList().ForEach(o => AddOperation(o));
            return this;
        }

        /// <summary>
        /// Adds a "no-operation" <see cref="Operation"/> to the transaction.
        /// </summary>
        /// <returns>This object.</returns>
        public Transaction AddNop()
        {
            if (_operations.Find(o => o.Equals(Operation.Nop())) == null)
                AddOperation(Operation.Nop());
            return this;
        }

        /// <summary>
        /// Adds a public key as signer to the transaction.
        /// </summary>
        /// <param name="signer">The public key to add as signer to the transaction.</param>
        /// <returns>This object.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Transaction AddSigner(Buffer signer)
        {
            KeyPair.EnsurePublicKey(signer);

            _signers.Add(signer);
            return this;
        }

        /// <summary>
        /// Adds multiple public keys as signers to the transaction.
        /// </summary>
        /// <param name="signers">The public keys to add as signers to the transaction.</param>
        /// <returns>This object.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public Transaction AddSigners(IEnumerable<Buffer> signers)
        {
            if (signers == null)
                throw new ArgumentNullException(nameof(signers));

            foreach (var signer in signers)
                AddSigner(signer);

            return this;
        }

        /// <summary>
        /// Adds a <see cref="ISignatureProvider"/> to the transaction
        /// and the public key of the provider as a signer.
        /// </summary>
        /// <param name="signatureProvider">The <see cref="ISignatureProvider"/> to add to the transaction.</param>
        /// <returns>This object.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Transaction AddSignatureProvider(ISignatureProvider signatureProvider)
        {
            if (signatureProvider == null)
                throw new ArgumentNullException(nameof(signatureProvider));

            AddSigner(signatureProvider.PubKey);
            _signatureProviders.Add(signatureProvider);
            return this;
        }

        /// <summary>
        /// Signs the transaction with the given signature provider.
        /// Does <b>not</b> add the public key as a signer before signing. 
        /// </summary>
        /// <param name="signatureProvider">The <see cref="ISignatureProvider"/> to sign the transaction.</param>
        /// <returns>The <see cref="Signature"/> created by the provider.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Signature Sign(ISignatureProvider signatureProvider)
        {
            if (signatureProvider == null)
                throw new ArgumentNullException(nameof(signatureProvider));

            return signatureProvider.Sign(GetBufferToSign());
        }

        /// <summary>
        /// Signs the transaction. Accepts signatures as a parameter that
        /// were created beforehand.
        /// </summary>
        /// <param name="preSigned">Signature that were created beforehand. May be null or empty.</param>
        /// <returns>The <see cref="Signed"/> transaction.</returns>
        /// <exception cref="ChromiaException"></exception>
        public Signed Sign(IEnumerable<Signature> preSigned = null)
        {
            return Signed.From(this, preSigned?.ToList());
        }

        /// <summary>
        /// Signs the transaction. Accepts a signature as a parameter that
        /// was created beforehand.
        /// </summary>
        /// <param name="preSigned">Signature that was created beforehand.</param>
        /// <returns>The <see cref="Signed"/> transaction.</returns>
        /// <exception cref="ChromiaException"></exception>
        public Signed Sign(Signature preSigned)
        {
            return Signed.From(this, new List<Signature>() { preSigned });
        }

        private Buffer Encode(List<Signature> signatures = null)
        {
            var tx = new object[]
            {
                GetBody(),
                signatures == null ? Array.Empty<Buffer>() : signatures.Select(s => s.Hash).ToArray()
            };

            return Gtv.Encode(tx);
        }

        private List<Signature> GetSignatures(List<Signature> preSigned)
        {
            var buffer = GetBufferToSign();

            var signatures = new List<Signature>();
            var signedPubkeys = new List<Buffer>();
            if (preSigned != null)
            {
                foreach (var preSig in preSigned)
                {
                    if (!SignatureProvider.Verify(preSig, buffer))
                        throw new InvalidOperationException($"signature from \"{preSig.PubKey}\" invalid");

                    signedPubkeys.Add(preSig.PubKey);
                    signatures.Add(preSig);
                }
            }

            foreach (var provider in _signatureProviders)
                signatures.Add(provider.Sign(buffer));

            return signatures;
        }

        private Buffer GetBufferToSign()
        {
            return Gtv.Hash(GetBody());
        }

        private object GetBody()
        {
            if (_blockchainRID.IsEmpty)
                throw new InvalidOperationException($"blockchain rid not set");

            var tx = new object[]
            {
                _blockchainRID,
                _operations.Select(o => o.GetBody()).ToArray(),
                _signers.Select(s => (object)s).ToArray()
            };

            return tx;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if ((obj == null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                var b = (Transaction)obj;
                return TransactionRID() == b.TransactionRID()
                    && _signers.SequenceEqual(b._signers);
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return TransactionRID().GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Tx \"0x{TransactionRID().Parse()}\"";
        }

        /// <summary>
        /// Contains information about a signed Chromia transaction.
        /// </summary>
        public readonly struct Signed
        {
            /// <summary>
            /// The blockchain RID the transaction is valid for.
            /// </summary>
            public readonly Buffer BlockchainRID;

            /// <summary>
            /// The transaction hash.
            /// </summary>
            public readonly Buffer TransactionRID;

            /// <summary>
            /// The transaction gtv body.
            /// </summary>
            public readonly Buffer GtvBody;

            /// <summary>
            /// List of <see cref="Operation"/> that are included in the transaction.
            /// </summary>
            public readonly IReadOnlyCollection<Operation> Operations;

            /// <summary>
            /// List of signers that signed the transaction.
            /// </summary>
            public readonly IReadOnlyCollection<Buffer> Signers;

            /// <summary>
            /// List of signatures for the transaction.
            /// </summary>
            public readonly IReadOnlyCollection<Buffer> Signatures;

            /// <summary>
            /// Creates a signed transaction out of a transaction.
            /// Accepts signatures that were created beforehand.
            /// </summary>
            /// <param name="tx">The transaction to sign.</param>
            /// <param name="preSigned">List of signatures for the transaction that were created beforehand.</param>
            /// <returns>The signed transaction.</returns>
            /// <exception cref="ChromiaException"></exception>
            /// <exception cref="ArgumentNullException"></exception>
            public static Signed From(Transaction tx, List<Signature> preSigned)
            {
                if (tx == null)
                    throw new ArgumentNullException(nameof(tx));

                var signatures = tx.GetSignatures(preSigned);
                var gtvBody = tx.Encode(signatures);

                return new Signed(
                    tx._blockchainRID,
                    tx.TransactionRID(),
                    gtvBody,
                    tx._operations,
                    tx._signers,
                    signatures.Select(s => s.Hash).ToList()
                );
            }

            /// <summary>
            /// Decodes a buffer into a signed transaction.
            /// </summary>
            /// <param name="buffer">The buffer to decode.</param>
            /// <returns>The signed transaction.</returns>
            /// <exception cref="ChromiaException"></exception>
            public static Signed From(Buffer buffer)
            {
                var obj = Gtv.Decode(buffer) as object[];
                var body = obj[0] as object[];

                var blockchainRID = (Buffer)body[0];
                var operations = (body[1] as object[]).Select(o => Operation.Decode(o as object[])).ToList();
                var signers = (body[2] as object[])?.Select(s => (Buffer)s).ToHashSet() ?? new HashSet<Buffer>();
                var signatures = (obj[1] as object[])?.Select(s => (Buffer)s).ToList() ?? new List<Buffer>();


                return new Signed(
                    blockchainRID,
                    Gtv.Hash(body),
                    buffer,
                    operations,
                    signers,
                    signatures
                );
            }

            private Signed(
                Buffer blockchainRID,
                Buffer transactionRID,
                Buffer gtvBody,
                List<Operation> operations,
                HashSet<Buffer> signers,
                List<Buffer> signatures
            )
            {
                BlockchainRID = blockchainRID;
                TransactionRID = transactionRID;
                GtvBody = gtvBody;
                Operations = operations;
                Signers = signers;
                Signatures = signatures;
            }

            /// <summary>
            /// Signs the signed transaction and adds the signature.
            /// </summary>
            /// <param name="signatureProvider"></param>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentException"></exception>
            public Signed Sign(ISignatureProvider signatureProvider)
            {
                if (signatureProvider == null)
                    throw new ArgumentNullException(nameof(signatureProvider));
                if (!Signers.Contains(signatureProvider.PubKey))
                    throw new ArgumentException("signature provider not a valid signer");

                var newTx = Build(BlockchainRID)
                    .AddOperations(Operations)
                    .AddSigners(Signers)
                    .AddSignatureProvider(signatureProvider);

                var sigList = Signatures.ToList();
                var signers = Signers;
                var body = TransactionRID;
                var signatures = sigList.Select(s => new Signature(signers.First(signer => SignatureProvider.Verify(signer, s, body)), s));
                return newTx.Sign(signatures);
            }

            /// <inheritdoc/>
            public override bool Equals(object obj)
            {
                if ((obj == null) || !GetType().Equals(obj.GetType()))
                {
                    return false;
                }
                else
                {
                    var tx = (Signed)obj;
                    return GtvBody == tx.GtvBody;
                }
            }

            /// <inheritdoc/>
            public static bool operator ==(Signed left, Signed right)
            {
                return left.Equals(right);
            }

            /// <inheritdoc/>
            public static bool operator !=(Signed left, Signed right)
            {
                return !(left == right);
            }

            /// <inheritdoc/>
            public override int GetHashCode()
            {
                return GtvBody.GetHashCode();
            }
        }
    }
}
