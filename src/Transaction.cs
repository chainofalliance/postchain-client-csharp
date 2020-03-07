using Cryptography.ECDSA;
using System.Threading.Tasks;

namespace Chromia.Postchain.Client
{
    public class Transaction
    {
        internal Gtx GtxObject;
        private RESTClient RestClient;

        internal Transaction(Gtx gtx, RESTClient restClient)
        {
            this.GtxObject = gtx;
            this.RestClient = restClient;
        }

        public void Sign(byte[] privKey, byte[] pubKey)
        {
            byte[] pub = pubKey;
            if(pubKey == null)
            {
                pub = Secp256K1Manager.GetPublicKey(privKey, true);
            }
            this.GtxObject.Sign(privKey, pub);
        }

        public string GetTxRID()
        {
            return PostchainUtil.ByteArrayToString(this.GetBufferToSign());
        }

        public byte[] GetBufferToSign()
        {
            return this.GtxObject.GetBufferToSign();
        }
        
        private void AddSignature(byte[] pubKey, byte[] signature)
        {
            this.GtxObject.AddSignature(pubKey, signature);
        }

        ///<summary>
        ///Add an operation to the Transaction.
        ///</summary>
        ///<param name = "name">Name of the operation.</param>
        ///<param name = "args">Array of dynamic parameters. For example {"Hamburg", 42}</param>
        public void AddOperation(string name, params dynamic[] args)
        {
            this.GtxObject.AddOperationToGtx(name, args);
        }

        ///<summary>
        ///Commit the Transaction and send it to the blockchain.
        ///</summary>
        ///<returns>Task, which returns null if it was succesful or the error message if not.</returns>
        public async Task<PostchainErrorControl> PostAndWaitConfirmation()
        {
            return await this.RestClient.PostAndWaitConfirmation(this.GtxObject.Serialize(), this.GetTxRID());
        }

        public async void Send()
        {
            var gtxBytes = this.GtxObject.Serialize();
            await this.RestClient.PostTransaction(gtxBytes);
            this.GtxObject = null;
        }

        public string Encode()
        {
            return this.GtxObject.Serialize();
        }
    }
}