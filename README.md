# Postchain Client C#
Postchain Client is a set of predefined functions and utilities offering a convenient and simplified interface for interacting with a decentralized application (dapp) built using the Postchain blockchain framework, also known as Chromia.

## Usage
### Native
The Postchain client can be installed from [nuget](https://www.nuget.org/packages/PostchainClient) or referenced through DLLs from the [releases](https://github.com/chainofalliance/postchain-client-csharp/releases).

### Unity
The NET Standard 2.1 DLL is compatible and can be imported into Unity. Unpack the contents from the [releases](https://github.com/chainofalliance/postchain-client-csharp/releases) section and add them to the Unity project.

### Unity WebGL
WebGL is not compatible with `System.Net` used in the DefaultTransport. You may need to implement an own Transport or use `Unity/UnityTransport.cs`. In order to inject it into the client code the following code needs to be executed before a `ChromiaClient` is created.
```c#
ChromiaClient.SetTransport(new UnityTransport());
```

## Initializing the client
From blockchain RID or blockchain IID (chain id). Also accepts a collection of node urls, where requests get evenly distributed on. Should a request to one node fail, the other nodes will be tried to access.
```c#
var blockchainRID = Buffer.From("7d565d92fd15bd1cdac2dc276cbcbc5581349d05a9e94ba919e1155ef4daf8f9");

var client1 = await ChromiaClient.Create("http://localhost:7740", blockchainRID);
var client2 = await ChromiaClient.Create("http://localhost:7740", 0);
var client3 = await ChromiaClient.Create(new() {"http://localhost:7740", "http://localhost:7741"}, 0);
```

Connecting to a network is achieved through the Directory System Chain. `CreateFromDirectory` accepts an array of URLs to system nodes running the directory chain. This directory chain is automatically queried to determine the URLs of the nodes in the network running the target dapp.
```c#
var blockchainRID = Buffer.From("7d565d92fd15bd1cdac2dc276cbcbc5581349d05a9e94ba919e1155ef4daf8f9");

var client1 = await ChromiaClient.CreateFromDirectory("http://localhost:7750", blockchainRID);
var client2 = await ChromiaClient.CreateFromDirectory("http://localhost:7750", 42);
var client3 = await ChromiaClient.Create(new() {"http://localhost:7750", "http://localhost:7751"}, 0);
```

## Queries
Queries return dapp data from the blockchain. They are invoked with typed parameters or an object implementing `IGtvSerializable`. Properties in the query object can be mapped to Rell query parameters through the `JsonProperty` attribute. It automatically parses the data to the given type.

```c#
[PostchainSerializable]
struct QueryParams
{
    [PostchainProperty("zip")]
    public int Zip;
}

var response = client.Query<string>("get_city", ("zip", 22222));
response = client.Query<string>("get_city", new QueryParams(){ Zip = 22222 });
```

## Transactions
To send transactions, begin by creating a simple signature provider. The signature provider is used to sign transactions.

```c#
var signer1 = SignatureProvider.Create(); // creates a new random keypair
var signer2 = SignatureProvider.Create(Buffer.Repeat('a', 32)); // from private key
```

Transactions send operations to the node that execute Rell code. Operations can be created dynamically or through the constructor. The parameters can also be passed as an object (see `PostchainSerializable`).

```c#
struct OperationParams
{
    [PostchainProperty("city")]
    public string City;
    [PostchainProperty("zip")]
    public int Zip;
}

var op1 = new Operation("insert_city", "hamburg", 22222);
var op2 = new Operation("insert_city")
    .AddParameter("hamburg")
    .AddParameter(22222);
var op3 = new Operation("insert_city", new OperationParams(){
    City = "hamburg",
    Zip = 22222
});
```

Transactions can be created dynamically per static method or through the client. Calling the `Sign` method signs the transaction by all added signature providers. A transaction can also be sent unsigned. If at least one signature provider is added to the transactions, `SendTransaction` signs the transaction before sending it. 
```c#
var op = new Operation("insert_city", "hamburg", 22222);
var tx1 = Transaction.Build()
    .AddOperation(op)
    .AddSignatureProvider(signer1);
client.SendTransaction(tx1);

var tx2 = client.TransactionBuilder()
    .AddOperation(op)
    .AddSignatureProvider(signer1);

var signedTx = tx2.Sign();
client.SendTransaction(signedTx);
```

Some transactions may need to be signed by multiple signers at different locations. The following transaction needs to be signed by two signers. One signer can create a `Signature` and share it with the other signer. They can import the signature and add it to the `Sign` method as a pre-signed signature.
```c#
var tx = Transaction.Build()
    .AddOperation(new Operation("insert_city", "hamburg", 22222))
    .AddSigner(signer1.PubKey)
    .AddSigner(signer2.PubKey);

var signature = tx.Sign(signer1);

// signer1 sends the signature.Hash and their pubkey to signer2

var signature = new Signature(signer1.PubKey, Buffer.From("<hash>"))
tx.AddSignatureProvider(signer2);
var signedTx = tx.Sign(signature);
client.SendTransaction(signedTx);
```

In order to create transactions with the same operations but different transaction RID, a "no-operation" (nop) operation can be added. Alternatively the transaction can be sent as a unique transaction which automatically adds a nop.
```c#
var tx = Transaction.Build()
    .AddOperation(new Operation("insert_city", "hamburg", 22222))
    .AddNop()
    .AddSignatureProvider(signer1.PubKey);

client.SendUniqueTransaction(tx);
```

## Error handling

In general, the client throws exceptions when it runs into error. The code is documented to show all possible exceptions thrown by each method. Logic errors by the client throw `ChromiaException` or its subclass `TransportException`. The `TransportException` contains information about which part of the transport failed and contains http status codes (if applicable).

## Type mapping

The following table shows which types map to each [Rell type](https://docs.chromia.com/rell/language-features/types/). Values that are `null` will be cast accordingly.
 
| Rell        | C#                | Notes                                                                                                       |
|-------------|-------------------|-------------------------------------------------------------------------------------------------------------|
| boolean     | bool              |                                                                                                             |
| integer     | long              | Internally long, because of the size of Rell integer. But any numeric type is supported  (int, byte, etc.). |
| big_integer | System.BigInteger |                                                                                                             |
| decimal     | string            | Internally string, but double and float values will automatically be cast.                                  |
| text        | string            |                                                                                                             |
| byte_array  | Chromia.Buffer    | Own struct with similar behavior to Javascript's Buffer. byte[] is also supported.                          |
| list<T>     | IList             | Any type that implements IList, i.e. List<T>, ReadOnlyList<T>, etc. Native arrays are supported as well.    |
| set<T>      | IList             | Any C# ISet type can be cast to a List or array with System.Linq.                                           |
| map<K,V>    | IDictionary       | Keys, if not of type string, will be converted using ToString(). Values can be any other type.              |
| struct    | `class` or `struct`       | Has to have attribute `PostchainSerializable`. Fields or properties that should be included in payload need `PostchainProperty`.           |

### Custom Types

As mentioned in the previous table, structs and classes can be converted directly into Rell structs. The class or struct needs to have the attribute `[PostchainSerializable]`. Properties and fields that need to be included in the payload need the attribute `[PostchainProperty]`. 

A `PostchainProperty` has to define the name it should be mapped to in Rell, as well as an optional `Order`. The latter is needed in the case `MyBigMixedClass` will get hashed directly using `ChromiaClient.Hash()`. In that case the names will be omitted and the order of fields and properties inside the class *should* get used. Since that order cannot be guaranteed if a mix of properties and fields is used, a manual order needs to be defined. 

Examples:
```csharp

    [PostchainSerializable]
    class MyBigClass
    {
        [PostchainProperty("s")]
        public string String;
        [PostchainProperty("ba")]
        public Buffer Buffer;
        [PostchainProperty("b")]
        public bool Bool;
        [PostchainProperty("i")]
        public int Int;
        [PostchainProperty("l")]
        public long Long;
        [PostchainProperty("f")]
        public float Float;
        [PostchainProperty("n")]
        public BigInteger BigInt;
        [PostchainProperty("e")]
        public MyEnum Enum;
    }

    [PostchainSerializable]
    class MyBigMixedClass
    {
        [PostchainProperty("s", 1)]
        public string String;
        [PostchainProperty("ba", 2)]
        private Buffer Buffer;
        [PostchainProperty("b", 3)]
        public bool Bool { get; }
        [PostchainProperty("i", 4)]
        public int Int { get; private set; }
        [PostchainProperty("l", 5)]
        private long Long { get; }
        [PostchainProperty("f", 6)]
        private float Float { get; set; }
        [PostchainProperty("n", 7)]
        public BigInteger BigInt;
        [PostchainProperty("e", 8)]
        public MyEnum Enum;
    }
```