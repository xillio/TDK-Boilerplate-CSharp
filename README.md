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
dotnet ./bin/Release/net6.0/TDK-Boilerplate-C#.dll
```

To implement your own connector, simply add another service in `src/service/` that extends `src/service/AbstractService.cs`. An example can be found in `src/service/FileService.cs`.

The service needs to implement the following functions:
- `validate`
- `authorize`
- `get`
- `getChildren`
- `getBinary`
- `create`

To use the newly created service, modify the creating of the `service` variable on line 22 of Controller.cs. The service property defines what file in src/service/ to use as active service.

The application currently only runs under HTTP. Implementing HTTPS could be achieved in several ways, either implement it in the codebase yourself or setup a proxy.

## Building an image

To build a docker image, run:
```
docker build -t <image_name> .
```

To run the image (`<configured port>` being the desired port):
```
docker run -p 8080:<configured_port> -v <host_path>:/contents <image_name>
```

The built container will expose port 8080 by default, this can be changed in `Dockerfile`. The `-v` option can be given to mount a host directory to the `/contents` folder in the container. This folder is used by the default `FileService` as repository. If running from the root of this git repository:
```
docker run -p 8080:8080 -v $(pwd)/contents:/contents <image_name>
```