using Cryptography.ECDSA;
using System.Threading.Tasks;

namespace Chromia.PostchainClient.GTX
{
    public class Transaction
    {
        public Gtx GtxObject;
        private RESTClient RestClient;

        public Transaction(Gtx gtx, RESTClient restClient)
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
            return Util.ByteArrayToString(Util.Sha256(this.GetBufferToSign()));
        }

        public byte[] GetBufferToSign()
        {
            return this.GtxObject.GetBufferToSign();
        }
        
        private void AddSignature(byte[] pubKey, byte[] signature)
        {
            this.GtxObject.AddSignature(pubKey, signature);
        }

        public void AddOperation(string name, dynamic[] args)
        {
            this.GtxObject.AddOperationToGtx(name, args);
        }

        public async Task<dynamic> PostAndWaitConfirmation()
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