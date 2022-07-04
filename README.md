# TDK-Boilerplate-CSharp



This project contains boilerplate to quickly setup a server to use the JSON RPC connector of the Xillio API.

You can read more regarding this connector in the [Xillio API documentation](https://docs.xill.io/#connector-json-rpc).

This TDK is also available in [Node](https://github.com/xillio/TDK-Boilerplate-Node)
and [Java](https://github.com/xillio/TDK-Boilerplate-Java).

## Supported operations

The TDK supports the following operations:
- Navigating the repository (getting entities)
- Downloading content
- Uploading translations

## How to use

As a simple test, the server can be started as follows:
```
dotnet build --configuration Release
docker build -t tdktest .
docker run -p 8080:8080 -v $(pwd)/contents:/contents tdktest
```

To implement your own connector, simply add another service in `src/service/` that extends `src/service/AbstractService.cs`. An example can be found in `src/service/FileService.cs`.

The service needs to implement the following functions:
- `validate`
- `authorize`
- `get`
- `getChildren`
- `getBinary`
- `create`

The application currently only runs under HTTP. Implementing HTTPS could be achieved in several ways, either implement it in the codebase yourself or setup a proxy.
