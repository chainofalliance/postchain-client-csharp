using Cryptography.ECDSA;
using System.Collections.Generic;
using System;
using RSG;

namespace Chromia.PostchainClient
{

    public class Gtxclient
    {
        private Restclient _restApiClient;
        private string _blockchainRID;
        private List<string> _functionNames;

        public Gtxclient(Restclient restApiClient, string blockchainRID, string[] functionNames)
        {
            this._restApiClient = restApiClient;
            this._blockchainRID = blockchainRID;
            this._functionNames = new List<string>(functionNames);
            this._functionNames.Add("message");
        }

        public Transaction NewTransaction(object[] signers)
        {
            Gtx newGtx = new Gtx(this._blockchainRID);

            foreach(byte[] signer in signers)
            {
                newGtx.AddSignerToGtx(signer);
            }

            Transaction req = new Transaction(newGtx, this._restApiClient);
            this.addFunctions(req);
            
            return req;
        }

        public Transaction TransactionFromRawTransaction(byte[] rawTransaction)
        {
            Gtx gtx = Gtx.Deserialize(rawTransaction);

            Transaction req = new Transaction(gtx, this._restApiClient);
            this.addFunctions(req);
            return req;
        }

        private void addFunctions(Transaction req)
        {
            foreach(string functionName in this._functionNames)
            {
                req._gtx.AddTransactionToGtx(functionName, 1);
            }
        }

        public class Transaction
        {
            public Gtx _gtx;
            private Restclient _restclient;
            public Transaction(Gtx gtx, Restclient restclient)
            {
                this._gtx = gtx;
                this._restclient = restclient;
            }

            public void Sign(byte[] privKey, byte[] pubKey)
            {
                byte[] pub = pubKey;
                if(pubKey == null)
                {
                    pub = Secp256K1Manager.GetPublicKey(privKey, false);
                }
                this._gtx.Sign(privKey, pub);
            }

            public string GetTxRID()
            {
                return System.Text.Encoding.Default.GetString(Util.Sha256(this.GetBufferToSign()));
            }

            public byte[] GetBufferToSign()
            {
                return this._gtx.GetBufferToSign();
            }

            public void AddSignature(byte[] pubKey, byte[] signature)
            {
                this._gtx.AddSignature(pubKey, signature);
            }

            public void AddOperation(string name, params object[] args)
            {
                this._gtx.AddTransactionToGtx(name, args);
            }

            public Promise<Promise<string>> PostAndWaitConfirmation()
            {
                return this._restclient.PostAndWaitConfirmation(
                    this._gtx.Serialize(), this.GetTxRID()
                );
            }

            public void Send(Action<string, dynamic> callback)
            {
                var gtxBytes = this._gtx.Serialize();
                this._restclient.PostTransaction(gtxBytes, callback);
                this._gtx = null;
                //?this.gtxBytes = gtxBytes;
            }

            public string Encode()
            {
                return this._gtx.Serialize();
            }
        }
    }


}