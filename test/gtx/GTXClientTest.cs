using Xunit;
using Chromia.Postchain.Client.GTX;
using System;
using System.Collections.Generic;

public interface FightLog
{
    int GetTime();
    int Type();
}

public struct Attributes 
{
    public int atk;
    public float chance;
}

public struct CharacterInfo : FightLog
{
    public int id;
    public string type;
    public Attributes stats;
    public List<int> synergyIDs;

    public int GetTime()
    {
        return 1337;
    }

    public int Type()
    {
        return id;
    }
}

namespace Chromia.Postchain.Client.Tests.GTX
{
    public class GTXClientTest
    {
        [Fact]
        public async void FullClientTest(){
            const string blockchainRID = "B6DC3118BED74B81EE83D1E8ED22160DBCB8DBE090B2F151285F4073C15AA3E5";

            var keyPair = Util.MakeKeyPair();
            var privKey = keyPair["privKey"];
            var pubKey = keyPair["pubKey"];

            var rest = new RESTClient("http://localhost:7740", blockchainRID);
            var gtx = new GTXClient(rest, blockchainRID);

            // var req = gtx.NewTransaction(new byte[][] {pubKey});

            // req.AddOperation("insert_city", "Hamburg", 22222);
            // req.AddOperation("create_user", pubKey, "Peter");
            // req.AddOperation("nop", new Random().Next());

            // req.Sign(privKey, pubKey);

            // var result = await req.PostAndWaitConfirmation();
            // if (result.Error)
            // {
            //     Console.WriteLine("Operation failed: " + result.ErrorMessage);
            // }
            
            var queryResult = await gtx.Query<CharacterInfo>("get_info");
            if (queryResult.control.Error)
            {
                Console.WriteLine(queryResult.control.ErrorMessage);
            }
            else
            {
                CharacterInfo info = queryResult.content;
                Console.WriteLine("Query1: " + info.GetTime());
                Console.WriteLine("Query1: " + info.Type());
                Console.WriteLine("Query2: " + info.type);
                Console.WriteLine("Query31: " + info.stats.atk);
                Console.WriteLine("Query32: " + info.stats.chance);
                foreach (var i in info.synergyIDs)
                {
                    Console.WriteLine("Query4: " + i);
                }
            }

            // var queryResult2 = await gtx.Query<string>("get_user_pubkey", ("name", "Peter"));
            // if (queryResult2.control.Error)
            // {
            //     Console.WriteLine(queryResult2.control.ErrorMessage);
            // }
            // else
            // {
            //     string queryPubkeyString = queryResult2.content;
            //     byte[] queryPubkey = Util.HexStringToBuffer(queryPubkeyString);
            //     Console.WriteLine("User Query: " + Util.ByteArrayToString(queryPubkey));
            // }
        }
    }
}
