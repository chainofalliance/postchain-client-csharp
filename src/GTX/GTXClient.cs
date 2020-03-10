using System;
using System.Threading.Tasks;

namespace Chromia.Postchain.Client
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
        public async Task<(T content, PostchainErrorControl control)> Query<T> (string queryName, params (string name, object content)[] queryObject)
        {
            var queryContent = await this.RestApiClient.Query<T>(queryName, queryObject);

            PostchainErrorControl queryError = new PostchainErrorControl();
            if (queryContent is HTTPStatusResponse)
            {
                var response = queryContent as HTTPStatusResponse;

                queryError.Error = true;
                queryError.ErrorMessage = response.message;

                return (default(T), queryError);
            }
            else
            {
                if (queryContent is T)
                {
                    return ((T) queryContent, queryError);
                }
                else
                {
                    queryError.Error = true;
                    queryError.ErrorMessage = "Can not cast query return to type";

                    return (default(T), queryError);
                }
            }
        }
    }
}