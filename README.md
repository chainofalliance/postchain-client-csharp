# Postchain Client API C#

## Compatible with Postchain 3.5.0 / Rell 0.10.9

## Changelog

### 0.5.1
- Add support to return `null` in queries.

<details>
<summary>Previous...</summary>

### 0.5.0
- Add `Gtx.Decode` to deserialize raw transactions.
- Refactor file structure
- Refactor tests

### 0.4.3
- Fix error message for failed transactions

### 0.4.2
- Add icon to package

### 0.4.1
- Update to .NET 5
- Fix bug where strings with UTF-8 characters could not be sent

### 0.4.0
First stable release
</details>

## Installation
### With NuGet:
```
Install-Package PostchainClient -Version 0.5.1
```
### With .NET CLI:
```
dotnet add package PostchainClient --version 0.5.1
```

For more information, see https://www.nuget.org/packages/PostchainClient/0.5.1

## Usage
```c#
using Chromia.Postchain.Client;


var keyPair = PostchainUtil.MakeKeyPair();
var privKey = keyPair["privKey"];
var pubKey = keyPair["pubKey"];

// The lower-level client that can be used for any
// postchain client messages. It only handles binary data.
var rest = new RESTClient("http://localhost:7740/");

// Instead of updateing the BRID each time the Rell code is compiled,
// the blockchain can now be accessed throug its chain_id from the 
// run.xml file (<chain name="MyCahin" iid="0">)
var initResult = await rest.InitializeBRIDFromChainID(0);
if (initResult.Error)
{
    Console.WriteLine("Cannot connect to blockchain!");
    return;
}

// Create an instance of the higher-level gtx client. It will
// use the rest client instance
var gtx = new GTXClient(rest);

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
    Console.WriteLine("ZIP Query: " + plz);
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
    byte[] queryPubkey = PostchainUtil.HexStringToBuffer(queryPubkeyString);
    Console.WriteLine("User Query: " + PostchainUtil.ByteArrayToString(queryPubkey));
}
```

### Rell file

```
entity city { key name; zip: integer; }
entity user { pubkey; name; }

operation insert_city (name, zip: integer) {
    create city (name, zip);
}

operation create_user(pubkey, name){
    create user (pubkey,name);
}

query get_city(name) = city @ {name}.zip;

query get_zip(zip: integer) = city @ {zip}.name;

query get_user_pubkey(name) = user @ {name}.pubkey;
```

## Testing

Run the Rell project in PostchainClient.Tests.Blockchain and execute `dotnet test` after.
