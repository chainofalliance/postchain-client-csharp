using System;
using System.Threading.Tasks;

namespace Chromia.Postchain.Client
{
    public class PostchainErrorControl
    {
        public PostchainErrorControl()
        {
            Error = false;
            ErrorMessage = "";
        }

        public PostchainErrorControl(bool error, string message)
        {
            Error = error;
            ErrorMessage = message;
        }

        public bool Error;
        public string ErrorMessage;
    }

    public class GTXClient
    {
        private RESTClient RestApiClient;

        ///<summary>
        ///Create new GTXClient object.
        ///</summary>
        ///<param name = "restApiClient">Initialized RESTCLient.</param>
        ///<param name = "blockchainRID">RID of blockchain.</param>
        public GTXClient(RESTClient restApiClient)
        {
            this.RestApiClient = restApiClient;
        }

        ///<summary>
        ///Create a new Transaction.
        ///</summary>
        ///<param name = "signers">Array of signers (can be null).</param>
        ///<returns>New Transaction object.</returns>
        public Transaction NewTransaction(byte[][] signers)
        {
            Gtx newGtx = new Gtx(RestApiClient.BlockchainRID);

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

            if (queryContent is HTTPStatusResponse)
            {
                var response = queryContent as HTTPStatusResponse;

                return (default(T), new PostchainErrorControl(true, response.message));
            }
            else
            {
                if (queryContent is T)
                {
                    return ((T) queryContent, new PostchainErrorControl());
                }
                else
                {
                    return (default(T), new PostchainErrorControl(true, "Can not cast query return to type"));
                }
            }
        }
    }
}