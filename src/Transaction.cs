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

        ///<summary>
        ///Add an operation to the Transaction.
        ///</summary>
        ///<param name = "name">Name of the operation.</param>
        ///<param name = "args">Array of object parameters. For example {"Hamburg", 42}</param>
        public void AddOperation(string name, params object[] args)
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

        public void Sign(byte[] privKey, byte[] pubKey)
        {
            byte[] pub = pubKey;
            if(pubKey == null)
            {
                pub = Secp256K1Manager.GetPublicKey(privKey, true);
            }
            this.GtxObject.Sign(privKey, pub);
        }
        
        private string GetTxRID()
        {
            return PostchainUtil.ByteArrayToString(this.GetBufferToSign());
        }

        private byte[] GetBufferToSign()
        {
            return this.GtxObject.GetBufferToSign();
        }
        
        private void AddSignature(byte[] pubKey, byte[] signature)
        {
            this.GtxObject.AddSignature(pubKey, signature);
        }

        private async void Send()
        {
            var gtxBytes = this.GtxObject.Serialize();
            await this.RestClient.PostTransaction(gtxBytes);
            this.GtxObject = null;
        }

        private string Encode()
        {
            return this.GtxObject.Serialize();
        }
    }
}