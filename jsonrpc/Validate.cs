using TDK_Boilerplate_C_.jsonrpc;
using TDK_Boilerplate_C_.jsonrpc.response;
using System;

namespace TDK_Boilerplate_C_.jsonrpc;

public class Validate
{
     public Validate(){
         
     }

    public bool isNumber(T value) {
        return int.TryParse(value, out _) || 
            float.TryParse(value, out _) || 
            double.TryParse(value, out _);
    } 

    public bool isBoolean(T value){
        return typeof(value) == typeof(bool);
    }

    public bool isArray(T value){
        return typeof(value) == typeof(T[]);
    }

    public bool isArrayOf(T value){
        return; //TO DO
    }

    public bool isObject(T value){
        return typeof(value) == typeof(object);
    }

    public bool isString(T value){
        return typeof(value) == typeof(string) || value.instanceof(String);
    }

    public bool isDate(T value){
        return typeof(value) == typeof(DateTime);
    }


    public void Validate(RequestDto Body){


    }

    // Create error response object
    public ErrorDto getError(string message, int code, object data){
        return err = new ErrorDto(
            ProtocolVersion.V2_0, 
            Body.id ?? ' ',
            code ?? ErrorCodes.InvalidConfiguration,
            msg ?? "Invalid request body",
            data); 
    }

    // Validate RPC header
    public ErrorDto validateRPCHeader(RequestDto body){

        if (Enum.IsDefined(typeof(ProtocolVersion), body.jsonrpc))
            return getError("Unsupported or missing JSON-RPC protocol version");

        if (!isNumber(body.id) && !isString(body.id))
            return getError("Invalid or missing request identifier");

        if (Enum.IsDefined(typeof(Method), body.method))
            return getError("Unsupported or missing JSON-RPC method");

        if (!isObject(body.@params))
            return getError("Invalid or missing request parameters value");
        
    }

    // Validate config parameter
    public ErrorDto validateConfigParameter(RequestDto body){
        if (!isObject(body.@params.config))
            return getError("Invalid or missing config parameter"); 

        foreach (var c in body.@params.config)
            if (isArray(c) || isObject(c))
                return getError("Config parameter does not support array values or nested objects");
    }

    // Validate XDIP
    public ErrorDto validateXdip(object @params){
        if (!isString(@params.xdip))
            return getError("Invalid or missing XDIP parameter");

        // Validate XDIP as URL
        const URL url = new URL(@params.xdip);

        if (url.protocol != "xdip:")
            return getError("XDIP URLs must use the xdip: scheme");

         if (url.username || url.password)
            return getError("XDIP URLs cannot use user information");

        if (!url.hostname.match(/[a-zA-Z0-9][a-zA-Z0-9-]{0,253}[a-zA-Z0-9]/)) //TO DO: fix this
            return getError(
                "Configuration ids can only contain alphanumerics and dashes, " +
                "cannot start or end with a dash and must be between 2 and 255 characters long");
        
        if (url.port)
            return getError("XDIP URLs cannot use port specifications");

        foreach (var [name, value] in url.searchParams.entries()) { //TO DO: fix this
            if (!value)
                return getError("XDIP URLs cannot contain flags as query parameters.");

            if (!["language","version"].includes(name))
                return getError("XDIP URL contains invalid query parameters.");
        }

        if (url.hash)
            return getError("XDIP URLs cannot use fragment parameters");
    }

    // Validate Request Parameters
    public ErrorDTO validateReqParams(object @params){
        if (@params.requestParameters) 
            if (!isObject(@params.requestParameters)) 
                return getError("Invalid request parameters");
        
        if (@params.requestParameters.projectionScopes) {
                if (!isArrayOf(@params.requestParameters.projectionScopes, isString))
                    return getError("Invalid projection scopes parameter");

                // Here we just take the first element of projectionScopes.
                var scope = @params.requestParameters.projectionScopes[0];
                if (scope && !Object.values(ProjectionScope).includes(scope))
                    return getError("Invalid or unsupported projection scope", ErrorCodes.NO_SUCH_SCOPE, scope);
            }

        // Ignore projectionIncludes, projectionExcludes, offset and limit.
    }

    // Validate decoraters
    public ErrorDTO validateDecorater(string name, object @params) {
        if (!isObject(@params))
            return getError("Invalid decorater: '", name, "'");

        switch (name) {
            case "container":
                if (!isBoolean(@params.hasChildren))
                    return getError("Invalid or missing hasChildren parameter of container decorator");

                break;

            case "contentType":
                if (@params.displayName && !isString(@params.displayName))
                    return getError("Invalid displayName parameter of contentType decorator");

                if (!isString(@params.systemName))
                    return getError("Invalid or missing systemName parameter of contentType decorator");

                break;

            case "created":
                if (!isDate(@params.date))
                    return getError("Invalid or missing date parameter of created decorator");

                break;

            case "file":
                if (!isString(@params.rawExtension))
                    return getError("Invalid or missing rawExtension parameter of file decorator");

                if (!isNumber(@params.size))
                    return getError("Invalid or missing size parameter of file decorator");

                break;

            case "language":
                if (!isString(@params.tag))
                    return getError("Invalid or missing tag parameter of language decorator");

                if (@params.translationOf && !isString(@params.translationOf))
                    return getError("Invalid translationOf parameter of language decorator");

                break;

            case "mimeType":
                if (!isString(@params.type))
                    return getError("Invalid or missing type parameter of mimeType decorator");

                break;

            case "modified":
                if (!isDate(@params.date))
                    return getError("Invalid or missing date parameter of modified decorator");

                break;

            case "name":
                if (@params.displayName && !isString(@params.displayName))
                    return getError("Invalid displayName parameter of name decorator");

                if (!isString(@params.systemName))
                    return getError("Invalid or missing systemName parameter of name decorator");

                break;

            case "parent":
                if (!isString(@params.id))
                    return getError("Invalid or missing id parameter of parent decorator");

                break;

            // Ignore unknown decorators.
        }
    }
    
    // Validate Entity
    public ErrorDTO validateEntity(object @params){
        if (!isObject(@params.entity))
            return getError("Invalid or missing entity parameter");

        if (!Object.values(EntityKind).includes(@params.entity.kind))
            return getError("Unsupported or missing entity kind parameter");

        if (!isObject(@params.entity.original))
            return getError("Invalid or missing entity original parameter");

        for (var [name, decParams] of Object.entries(@params.entity.original)) { //TO DO: fix this
            var err = validateDecorator(name, decParams);
            if (err) return err;
        }

        // TODO: What decorators are even required?
        if (!@params.entity.original.name)
            return getError("Missing decorator", ErrorCodes.MISSING_DECORATOR, decorator: "name" );

        if (!@params.entity.original.language)
            return getError("Missing decorator", ErrorCodes.MISSING_DECORATOR,  decorator: "language" );

    }
    /*function validateEntity(params) {
        if (!isObject(params.entity))
            return getError('Invalid or missing entity parameter');

        if (!Object.values(EntityKind).includes(params.entity.kind))
            return getError('Unsupported or missing entity kind parameter');

        if (!isObject(params.entity.original))
            return getError('Invalid or missing entity original parameter');

        for (const [name, decParams] of Object.entries(params.entity.original)) {
            const err = validateDecorator(name, decParams);
            if (err) return err;
        }

        // TODO: What decorators are even required?
        if (!params.entity.original.name)
            return getError('Missing decorator', ErrorCodes.MISSING_DECORATOR, { decorator: 'name' });

        if (!params.entity.original.language)
            return getError('Missing decorator', ErrorCodes.MISSING_DECORATOR, { decorator: 'language' });

    }*/
}

