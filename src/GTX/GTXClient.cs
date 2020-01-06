using System;
using System.Threading.Tasks;

namespace Chromia.Postchain.Client.GTX
{
    public struct PostchainErrorControl
    {
        public bool Error;
        public string ErrorMessage;
    }

    public class GTXClient
    {
        private RESTClient RestApiClient;
        private string BlockchainRID;

        ///<summary>
        ///Create new GTXClient object.
        ///</summary>
        ///<param name = "restApiClient">Initialized RESTCLient.</param>
        ///<param name = "blockchainRID">RID of blockchain.</param>
        public GTXClient(RESTClient restApiClient, string blockchainRID)
        {
            this.RestApiClient = restApiClient;
            this.BlockchainRID = blockchainRID;
        }

        ///<summary>
        ///Create a new Transaction.
        ///</summary>
        ///<param name = "signers">Array of signers (can be null).</param>
        ///<returns>New Transaction object.</returns>
        public Transaction NewTransaction(byte[][] signers)
        {
            Gtx newGtx = new Gtx(this.BlockchainRID);

            foreach(byte[] signer in signers)
            {
                newGtx.AddSignerToGtx(signer);
            }

            Transaction req = new Transaction(newGtx, this.RestApiClient);
            
            return req;
        }

        ///<summary>
        ///Send a query to the node.
        ///</summary>
        ///<param name = "queryName">Name of the query to be called.</param>
        ///<param name = "queryObject">List of parameter pairs of query parameter name and its value. For example {"city", "Hamburg"}.</param>
        ///<returns>Task, which returns the query return content.</returns>
        public async Task<(T content, PostchainErrorControl control)> Query<T> (string queryName, params dynamic[] queryObject)
        {
            var queryContent = await this.RestApiClient.Query(queryName, queryObject);
            Console.WriteLine("t: " + queryContent);

            PostchainErrorControl queryError = new PostchainErrorControl();
            try
            {
                queryError.Error = queryContent.__postchainerror__;
                queryError.ErrorMessage = queryContent.message;

                return (default(T), queryError);
            }
            catch
            {
                queryError.Error = false;
                queryError.ErrorMessage = "";                
            }

            T contentObject;
            try
            {
                if (queryContent.GetType().IsPrimitive || queryContent.GetType().Equals(typeof(String)))
                {
                    contentObject = (T) queryContent;
                }
                else
                {
                    contentObject = queryContent.ToObject<T>();
                }
            }
            catch (System.Exception e)
            {
                contentObject = default(T);
                queryError.Error = true;
                queryError.ErrorMessage = e.Message;
            }

            return (contentObject, queryError);
        } 

        /*
        [Obsolete]
        public Transaction TransactionFromRawTransaction(byte[] rawTransaction)
        {
            Gtx gtx = Gtx.Deserialize(rawTransaction);

            Transaction req = new Transaction(gtx, this.RestApiClient);

            return req;
        }
        */
    }
}