# Postchain Client API C#

## Compatible with Postchain 2 / Rell 0.8

## Installation
### With NuGet:
```
Install-Package PostchainClient -Version 0.1.0
```
### With .NET CLI:
```
dotnet add package PostchainClient --version 0.1.0
```

For more information, see https://www.nuget.org/packages/PostchainClient/0.1.0

## Usage
```c#
using Chromia.PostchainClient;
using Chromia.PostchainClient.GTX;

const string blockchainRID = "78967baa4768cbcef11c508326ffb13a956689fcb6dc3ba17f4b895cbb1577a3";

// Generates a new key pair.
var keyPair = Util.MakeKeyPair();
var privKey = keyPair["privKey"];
var pubKey = keyPair["pubKey"];

// The lower-level client that can be used for any
// postchain client messages. It only handles binary data.
var rest = new RESTClient("http://localhost:7740", blockchainRID);

// Create an instance of the higher-level gtx client. It will
// use the rest client instance and it will allow to generate
// transactions and query data.
var gtx = new GTXClient(rest, blockchainRID);

// Start a new request. A request instance is created.
// The public keys are the keys that must sign the request
// before sending it to postchain. Can be empty.
var req = gtx.NewTransaction(new byte[][] {pubKey});

// Add operations to transaction
req.AddOperation("insert_city", "Hamburg", 223232);
req.AddOperation("create_user", pubKey, "Peter");

// Sign transaction with key pair
req.Sign(privKey, pubKey);

// Commit transaction to node. If it fails it returns the 
// corresponding error message.
var result = await req.PostAndWaitConfirmation();
Console.WriteLine("Operation: " + result);


// Query data to see if it was inserted correctly
result = await gtx.Query("get_city", ("name", "Hamburg"));
Console.WriteLine("Query: " + result);

result = await gtx.Query("get_plz", ("plz", 223232));
Console.WriteLine("Query2: " + result);

result = await gtx.Query("get_user_name", ("pubkey", pubKey));
Console.WriteLine("Query3: " + result);
```

### Rell file
```
class city { key name; plz: integer; }
class user {pubkey; name;}

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

query get_user_name(pubkey){
    return user @ {pubkey}.name;
}
```
