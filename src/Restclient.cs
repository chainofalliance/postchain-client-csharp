﻿using System;
using System.Text;

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

        public void getTransaction(string messageHash, Action<string, dynamic> callback){
            _validateMessageHash(messageHash);
            Action<string,int,dynamic> cb = delegate(string error, int statusCode, dynamic responseObject){
                _handleGetResponse(error, statusCode, statusCode == 200 ? StringToHex(responseObject["tx"].ToString()) : null, callback);
            };

            _get(this._urlBase, "tx/" + this._blockhainRID + "/" + StringToHex(messageHash), cb);
        }

        public void postTransaction(string serializedTransaction, Action<string, dynamic> callback){
            string jsonString = @"{tx: " + StringToHex(serializedTransaction) + "}";
            var jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jsonString);

            _doPost(this._urlBase, "tx/" + this._blockhainRID, jsonObject, callback);
        }

        public void getConfirmationProof(string messageHash, Action<string, string> callback){
            _validateMessageHash(messageHash);
            Action<string,int,dynamic> cb = delegate(string error, int statusCode, dynamic responseObject){
                if (statusCode == 200){
                    responseObject["hash"] = _b(responseObject["hash"].ToString());
                    responseObject["blockHeader"] = _b(responseObject["blockHeader"].ToString());
                    if (responseObject["signatures"].ToString() != ""){
                        for (int i = 0; i < responseObject["signatures"].Count; i++)
                        {
                            responseObject["signatures"][i]["pubKey"] = _b(responseObject["signatures"][i]["pubKey"].ToString());
                            responseObject["signatures"][i]["signature"] = _b(responseObject["signatures"][i]["signature"].ToString());
                        }
                    }

                    if (responseObject["merklePath"].ToString() != ""){
                        for (int i = 0; i < responseObject["merklePath"].Count; i++)
                        {
                            responseObject["merklePath"][i]["hash"] = _b(responseObject["merklePath"][i]["hash"].ToString());
                        }
                    }
                }
            };

            _get(_urlBase, "tx/" + this._blockhainRID + "/" + StringToHex(messageHash) + "/confirmationProof", cb);
        }

        public void status(string messageHash, Action<string, dynamic> callback){
            _validateMessageHash(messageHash);
            Action<string,int,dynamic> cb = delegate(string error, int statusCode, dynamic responseObject){
                _handleGetResponse(error, statusCode, responseObject, callback);
            };

            _get(this._urlBase, "tx/" + this._blockhainRID + "/" + StringToHex(messageHash) + "/status", cb);
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

        private void _doPost(string config, string path, string jsonObject, Action<string, dynamic> responseCallback){
            throw new NotImplementedException("Please create a test first.");
        }

        private void _get(string config, string path, Action<string, int, dynamic> callback){
            throw new NotImplementedException("Please create a test first.");
        }

        private void _post(string config, string path, string jsonBody, Action<string, int, string> callback){
            throw new NotImplementedException("Please create a test first.");
        }

        private void _validateMessageHash(string messageHash){
            throw new NotImplementedException("Please create a test first.");
        }

        private void _handleGetResponse(string error, int statusCode, string responseObject, Action<string, dynamic> callback){
            throw new NotImplementedException("Please create a test first.");
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
        private string _b(string stringValue){
            throw new NotImplementedException("Please create a test first.");
        }
    }
}