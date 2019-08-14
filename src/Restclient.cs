using System;

namespace Chromia.PostchainClient
{
    public class Restclient
    {
        private string _urlBase;
        private string _blockhainRID;
        private int _maxSockets;

        public Restclient(string urlBase, string blockhainRID, int maxSockets = 10){
            this._urlBase = urlBase;
            this._blockhainRID = blockhainRID;
            this._maxSockets = maxSockets;
        }

        public void getTransaction(string messageHash, Action<string, int, string> callback){
            throw new NotImplementedException("Please create a test first.");
        }

        public void postTransaction(string serializedTransaction, Action<string, int, string> callback){
            throw new NotImplementedException("Please create a test first.");
        }

        public void getConfirmationProof(string messageHash, Action<string, string> callback){
            throw new NotImplementedException("Please create a test first.");
        }

        public void status(string messageHash, Action<string, string> callback){
            throw new NotImplementedException("Please create a test first.");
        }

        public void query(string queryName, string queryObject){
            throw new NotImplementedException("Please create a test first.");
        }

        public void waitConfirmation(string txRID){
            throw new NotImplementedException("Please create a test first.");
        }

        public void postAndWaitConfirmation(string serializedTransaction, string txRID, bool validate){
            throw new NotImplementedException("Please create a test first.");
        }

        private void _doPost(string config, string path, string jsonObject, Action<string, string> responseCallback){
            throw new NotImplementedException("Please create a test first.");
        }

        private void _get(string config, string path, Action<string, int, string> callback){
            throw new NotImplementedException("Please create a test first.");
        }

        private void _post(string config, string path, string jsonBody, Action<string, int, string> callback){
            throw new NotImplementedException("Please create a test first.");
        }

        private void _validateMessageHash(string messageHash){
            throw new NotImplementedException("Please create a test first.");
        }

        private void _handleGetResponse(bool error, int statusCode, string responseObject, Action<string, string> callback){
            throw new NotImplementedException("Please create a test first.");
        }

        private void _b(string stringValue){
            throw new NotImplementedException("Please create a test first.");
        }
    }
}