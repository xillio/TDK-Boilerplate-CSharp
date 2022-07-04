using TDK_Boilerplate_C_.jsonrpc;
using TDK_Boilerplate_C_.jsonrpc.response;
using TDK_Boilerplate_C_.Service;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.StaticFiles;
using System.Net.Mime;
using System.IO;

namespace TDK_Boilerplate_C_.jsonrpc;

public class Execute {
    private RequestDto body;
    private AbstractService service;
    public Execute(RequestDto body, AbstractService service){
        this.body = body;
        this.service = service;
    }

    public asEntityReturnType asEntity(Output input){
        bool isFile = !input.isFolder;
        bool isFolder = input.isFolder;

        Decorators decorators = new Decorators {
            container = isFile ? null : new container{
                hasChildren = true
            },
            contentType = new contentType{
                systemName = isFile ? EntityKind.FILE : EntityKind.FOLDER
            },
            created = new created{
                date = input.created 
            },
            language = isFolder ? null : new language{
                tag = "en-US"
            },
            mimeType = isFolder ? null : new mimeType{
                type = GetMimeTypeForFileExtension(input.systemName)
            },
            file = isFolder ? null : new file{
                rawExtension = Path.GetExtension(input.systemName ?? ""),
                size = input.size
            },
            modified = new modified{
                date = input.modified 
            },
            name = new name{
                displayName = "", //input.displayName
                systemName = input.systemName
            },
            parent = new parent{
                id = Path.GetDirectoryName(input.xdip.ToString() ?? "")
            }
        };

        return new asEntityReturnType {
            id = input.xdip.ToString() ?? "",
            xdip = input.xdip,
            kind = isFile ? EntityKind.FILE : EntityKind.FOLDER,
            original = decorators,
            modified = decorators
        };
    }

    public string GetMimeTypeForFileExtension(string filePath) {
        const string DefaultContentType = "application/octet-stream";
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(filePath, out string contentType))
        {
            contentType = DefaultContentType;
        }
        return contentType;
    }

    public ResponseDto getError(string msg, ErrorCode code){
        ResponseDto err = new ResponseDto{
            jsonrpc = ProtocolVersion.V2_0, 
            id = body.id ?? "",
            error = new ErrorDto{
                code = (int)code,
                message = msg,
                data = null
            }
        };
        return err;
    }

    public ResponseDto getResponse(object result){
        ResponseDto res = new ResponseDto{
            jsonrpc = ProtocolVersion.V2_0, 
            id = body.id ?? "",
            result = new ResultDto{
                result = result
            }
        };
        return res;
    }

    public ResponseDto? run(){
        object? config = getProperty(body.@params, "config");
        try{
            if (!service.validate(config)){
                return getError("invalid config parameter", ErrorCode.INVALID_CONFIGURATION);
            }
            if (!service.authorize(config)){
                return getError("Failed to authorize request", ErrorCode.AUTHORIZATION_FAILED);
            }

            if (body.method == Method.ENTITY_GET){
                return getResponse( entityGet() ); 
            }
            if (body.method == Method.ENTITY_GET_BINARY){
                return getResponse( entityGetBinary() );
            }
            if (body.method == Method.ENTITY_CREATE){
                return getResponse( entityCreate() );
            }
        }
        catch(Exception err){
            return getError(err.Message, (ErrorCode)err.HResult);
        }
        return null;
    }

    public object? getProperty(object? obj, string name)
    {
        if (obj == null) return null;
        if (((JsonElement)obj).ValueKind != JsonValueKind.Object) return null;

        JsonElement tmp;
        if (!((JsonElement)obj).TryGetProperty(name, out tmp)) return null;

        return tmp;
    } 

    public string[] getPropertyNames(object? obj){
        if (obj == null) return new string[]{};
        if (((JsonElement)obj).ValueKind != JsonValueKind.Object) return new string[]{};
        return ((JsonElement)obj).EnumerateObject().Select(v => v.Name).ToArray();
    }

    public object[] getProperties(object? obj)
    {
        if (obj == null) return new object[]{};
        if (((JsonElement)obj).ValueKind == JsonValueKind.Object) 
        {
            return ((JsonElement)obj).EnumerateObject().Select(v => (object)v.Value).ToArray();
        }
        else if (((JsonElement)obj).ValueKind == JsonValueKind.Array) 
        {
            return ((JsonElement)obj).EnumerateArray().Select(v => (object)v).ToArray();
        }
        return new object[]{};
    }

    public entityGetReturnType entityGet() {
        object? requestParameters = getProperty(body.@params, "requestParameters");
        object? projectionScopes = getProperty(requestParameters, "projectionScopes");
        object[] projectionScopesProperties = getProperties(projectionScopes);
        object? scope = projectionScopesProperties.Length > 0 ? projectionScopesProperties[0] : null;

        object? config = getProperty(body.@params, "config");
        object? xdip = getProperty(body.@params, "xdip");
        
        if (scope?.ToString() == ProjectionScope.PATH_CHILDREN_REFERENCE){
            var children = service.getChildren(config, xdip);
            var result = new List<entityId>();
            foreach (Output o in children){
                result.Add(new entityId {
                    id = asEntity(o).id
                });
            }
            return new entityGetReturnType {
                path_children_reference = result
            };
        }
        else if (scope?.ToString() == ProjectionScope.PATH_CHILDREN_ENTITY){
            var children = service.getChildren(config, xdip);
            var result = new List<asEntityReturnType>{};
            foreach (Output o in children)
            {
                result.Add(asEntity(o));
            }
            return new entityGetReturnType {
                path_children_entity = result
            };
        }
        else {
            var result = asEntity(service.get(config, xdip));
            scope = ProjectionScope.ENTITY;
            return new entityGetReturnType {
                entity = result
            };
        }
    }

    public string entityGetBinary() {
        object? config = getProperty(body.@params, "config");
        object? xdip = getProperty(body.@params, "xdip");

        var result = service.getBinary(config, xdip);
        return System.Convert.ToBase64String(Encoding.UTF8.GetBytes(result ?? ""));
    }

    public entityCreateReturnType entityCreate() {
        object? config = getProperty(body.@params, "config");
        object? entity = getProperty(body.@params, "entity");
        object? binaryContents = getProperty(body.@params, "binaryContents");

        var bin = System.Convert.ToBase64String(Encoding.UTF8.GetBytes(binaryContents.ToString() ?? ""));

        var result = service.create(config, entity, bin);
        return new entityCreateReturnType {
            entity = asEntity(result)
        };
    }
}

public class entityCreateReturnType {
    public asEntityReturnType entity { get; set; }
}

public class entityGetReturnType {
    public asEntityReturnType? entity { get; set;}
    public List<entityId>? path_children_reference { get; set;}
    public List<asEntityReturnType>? path_children_entity { get; set;}
}

public class entityId {
    public string id { get; set;}
}

public class asEntityReturnType {
    public string id { get; set;}
    public object xdip { get; set; }
    public string kind { get; set; }
    public Decorators original { get; set; }
    public Decorators modified { get; set; }
}