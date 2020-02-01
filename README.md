# Postchain Client API C#

## Compatible with Postchain 3.1.0 / Rell 0.10.1

## Installation
### With NuGet:
```
Install-Package PostchainClient -Version 0.3.6
```
### With .NET CLI:
```
dotnet add package PostchainClient --version 0.3.6
```

For more information, see https://www.nuget.org/packages/PostchainClient/0.3.6

## Usage
```c#
using Chromia.PostchainClient;
using Chromia.PostchainClient.GTX;

// RID for the rell code below
const string blockchainRID = "AC651CC730397A6880AD7695E73663720068532D7406F0BA0753C2F65A9AD169";

var keyPair = Util.MakeKeyPair();
var privKey = keyPair["privKey"];
var pubKey = keyPair["pubKey"];

// The lower-level client that can be used for any
// postchain client messages. It only handles binary data.
var rest = new RESTClient("http://localhost:7740", blockchainRID);

// Create an instance of the higher-level gtx client. It will
// use the rest client instance
var gtx = new GTXClient(rest, blockchainRID);

// Start a new request. A request instance is created.
// The public keys are the keys that must sign the request
// before sending it to postchain. Can be empty.

var req = gtx.NewTransaction(new byte[][] {pubKey});

req.AddOperation("insert_city", "Hamburg", 22222);
req.AddOperation("create_user", pubKey, "Peter");

// Since transactions with the same operations will result in the same txid,
// transactions can contain "nop" operations. This is needed to satisfy
// the unique txid constraint of the postchain. 
req.AddOperation("nop", new Random().Next());

req.Sign(privKey, pubKey);

var result = await req.PostAndWaitConfirmation();
if (result.Error)
{
    Console.WriteLine("Operation failed: " + result.ErrorMessage);
}

// The expected return type has to be passed to the query function. This
// also works with complex types (i.e. your own struct as well as lists).
// The returned tuple will consist of (content, control). The content is of
// the type you pass the function. The control struct contains an error flag
// as well as the error message.
var queryResult = await gtx.Query<int>("get_city", ("name", "Hamburg"));
if (queryResult.control.Error)
{
    Console.WriteLine(queryResult.control.ErrorMessage);
}
else
{
    int plz = queryResult.content;
    Console.WriteLine("PLZ Query: " + plz);
}

// Same as above with the exception that byte arrays will be returned as strings.
// To convert it to a byte array, use the util function Util.HexStringToBuffer() 
// in the Chromia.Postchain.Client.GTX namespace.
var queryResult2 = await gtx.Query<string>("get_user_pubkey", ("name", "Peter"));
if (queryResult2.control.Error)
{
    Console.WriteLine(queryResult2.control.ErrorMessage);
}
else
{
    string queryPubkeyString = queryResult2.content;
    byte[] queryPubkey = Util.HexStringToBuffer(queryPubkeyString);
    Console.WriteLine("User Query: " + Util.ByteArrayToString(queryPubkey));
}
```

### Rell file
```
entity city { key name; plz: integer; }
entity user {pubkey; name;}

operation insert_city (name, plz: integer) {
    create city (name, plz);
}

query get_city(name){
    return city @ {name}.plz;
}


query get_plz(plz: integer){
    return city @ {plz}.name;
}

operation create_user(pubkey, name){
    create user (pubkey,name);
}

query get_user_pubkey(name){
    return user @ {name}.pubkey;
}
```
