using TDK_Boilerplate_C_.jsonrpc;
using TDK_Boilerplate_C_.jsonrpc.response;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TDK_Boilerplate_C_.jsonrpc;

public class Validate
{
    private RequestDto body;

    public Validate(RequestDto body){
        this.body = body;
    }

    public bool isNumber(object? value) {
        return value != null && value.GetType() == typeof(JsonElement) && ((JsonElement)value).ValueKind == JsonValueKind.Number;
    } 

    public bool isBoolean(object? value){
        return value != null && 
            value.GetType() == typeof(JsonElement) && 
            (((JsonElement)value).ValueKind == JsonValueKind.True ||
            ((JsonElement)value).ValueKind == JsonValueKind.False);
    }

    public bool isArray(object? value){
        return value != null && value.GetType() == typeof(JsonElement) && ((JsonElement)value).ValueKind == JsonValueKind.Array;
    }

    public bool isObject(object? value){
        return value != null && value.GetType() == typeof(JsonElement) && ((JsonElement)value).ValueKind == JsonValueKind.Object;
    }

    public bool isString(object? value){
        return value != null && value.GetType() == typeof(JsonElement) && ((JsonElement)value).ValueKind == JsonValueKind.String;
    }

    public bool isDate(object? value){
        return value != null && value.GetType() == typeof(JsonElement) && ((JsonElement)value).ValueKind == JsonValueKind.String;
        //TO DO: check if date
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

    public string?[] getStaticStrings(Type obj){
        if (obj == null) return new string[]{};
        FieldInfo[] properties = obj.GetFields(BindingFlags.Static | BindingFlags.Public).Where(f => f.FieldType == typeof(string)).ToArray();
        return properties.Select(p => (string?)p.GetValue(obj)).ToArray();
    }

    public ResponseDto? doValidate(){
        ResponseDto? res;
        res = validateRPCHeader();
        if (res != null) return res;
        res = validateConfigParameter();
        if (res != null) return res;
        res = bodyMethod();
        if (res != null) return res;

        return null;
    }

    // Create error response object
    public ResponseDto getError(string msg, ErrorCode code = ErrorCode.INVALID_CONFIGURATION, object? data = null){
        ResponseDto err = new ResponseDto{
            jsonrpc = ProtocolVersion.V2_0, 
            id = body.id ?? "",
            error = new ErrorDto{
                code = (int)code,
                message = msg ?? "Invalid request body",
                // TODO: data not passed along.
                data = data
            }
        };
        return err;
    }

    // Validate RPC header
    public ResponseDto? validateRPCHeader(){

        if (!getStaticStrings(typeof(ProtocolVersion)).Contains(body.jsonrpc))
            return getError("Unsupported or missing JSON-RPC protocol version");

        if (!getStaticStrings(typeof(Method)).Contains(body.method))
            return getError("Unsupported or missing JSON-RPC method");

        if (!isObject(body.@params))
            return getError("Invalid or missing request parameters value");
        
        return null;
    }

    // Validate config parameter
    public ResponseDto? validateConfigParameter(){
        object? config = getProperty(body.@params, "config");
        if (!isObject(config))
            return getError("Invalid or missing config parameter"); 

        foreach (var c in getProperties(config))
            if (isArray(c) || isObject(c))
                return getError("Config parameter does not support array values or nested objects");

        return null;
    }

    // Validate XDIP
    public ResponseDto? validateXdip(){
        object? xdip = getProperty(body.@params, "xdip");
        string strXdip = xdip != null ? xdip.ToString() ?? "" : "";
        if (xdip == null || !isString(xdip))
            return getError("Invalid or missing XDIP parameter");

        // Validate XDIP as URL
        Uri url; 
        try 
        {
            url = new Uri(strXdip);
        }
        catch 
        {
            return getError("XDIP parameter must be an URI");
        }

        if (url.Scheme != "xdip")
            return getError("XDIP URLs must use the xdip: scheme");

        if (url.UserInfo.Length > 0)
            return getError("XDIP URLs cannot use user information");

        Regex req = new Regex(@"[a-zA-Z0-9][a-zA-Z0-9-]{0,253}[a-zA-Z0-9]");
        if (!req.IsMatch(url.Host))
            return getError(
                "Configuration ids can only contain alphanumerics and dashes, " +
                "cannot start or end with a dash and must be between 2 and 255 characters long");
        
        string query = url.Query;
        if (query.Length > 0){
            query = query.Remove(0, 1);
            string[] queries = query.Split('&');
            foreach (string s in queries){
                string[] keyValue = s.Split('=');
                if (keyValue.Length < 2)
                    return getError("XDIP URLs cannot contain flags as query parameters.");

                if (!new string[]{"language","version"}.Contains(keyValue[0]))
                    return getError("XDIP URL contains invalid query parameters.");
            }
        }

        if (url.Fragment.Length > 0)
            return getError("XDIP URLs cannot use fragment parameters");
       
        return null;
    }

    // Validate Request Parameters
    public ResponseDto? validateRequestParameters(){
        object? requestParameters = getProperty(body.@params, "requestParameters");
        if (requestParameters != null)
        {
            if (!isObject(requestParameters)) 
                return getError("Invalid request parameters");
        
            object? projectionScopes = getProperty(requestParameters, "projectionScopes");
            if (projectionScopes != null) {
                if (!isArray(projectionScopes))
                    return getError("Invalid projection scopes parameter");

                // Check if all strings.
                object[] projectionScopesProperties = getProperties(projectionScopes);

                foreach (object o in projectionScopesProperties){
                    if (!isString(o)){
                        return getError("Invalid projection scopes parameter");
                    }
                }

                // Here we just take the first element of projectionScopes.
                object? scope = projectionScopesProperties.Length > 0 ? projectionScopesProperties[0] : null;
                if (scope != null &&  !getStaticStrings(typeof(ProjectionScope)).Contains(scope.ToString()))
                    return getError("Invalid or unsupported projection scope", ErrorCode.NO_SUCH_SCOPE, scope);
            }
            // Ignore projectionIncludes, projectionExcludes, offset and limit.
        }
        return null;
    }

    // Validate decoraters
    public ResponseDto? validateDecorator(string name, object? decParams) {
        if (decParams == null || !isObject(decParams))
            return getError("Invalid decorater: '");

        object? date = getProperty(decParams, "date");
        object? hasChildren = getProperty(decParams, "hasChildren");
        object? displayName = getProperty(decParams, "displayName");
        object? systemName = getProperty(decParams, "systemName");
        object? rawExtension = getProperty(decParams, "rawExtension");
        object? size = getProperty(decParams, "size");
        object? tag = getProperty(decParams, "tag");
        object? translationOf = getProperty(decParams, "translationOf");
        object? type = getProperty(decParams, "type");
        object? id = getProperty(decParams, "id");

        switch (name) {
            case "container":
               
                if (!isBoolean(hasChildren))
                    return getError("Invalid or missing hasChildren parameter of container decorator");

                break;

            case "contentType":
                if (displayName != null && !isString(displayName))
                    return getError("Invalid displayName parameter of contentType decorator");

                if (!isString(systemName))
                    return getError("Invalid or missing systemName parameter of contentType decorator");

                break;

            case "created":
                if (!isDate(date))
                    return getError("Invalid or missing date parameter of created decorator");

                break;

            case "file":
                if (!isString(rawExtension))
                    return getError("Invalid or missing rawExtension parameter of file decorator");

                if (!isNumber(size))
                    return getError("Invalid or missing size parameter of file decorator");

                break;

            case "language":
                if (!isString(tag))
                    return getError("Invalid or missing tag parameter of language decorator");

                if (translationOf != null && !isString(translationOf))
                    return getError("Invalid translationOf parameter of language decorator");

                break;

            case "mimeType":
                if (!isString(type))
                    return getError("Invalid or missing type parameter of mimeType decorator");

                break;

            case "modified":
                if (!isString(date))
                    return getError("Invalid or missing date parameter of modified decorator");

                break;

            case "name":
                if (displayName != null && !isString(displayName))
                    return getError("Invalid displayName parameter of name decorator");

                if (!isString(systemName))
                    return getError("Invalid or missing systemName parameter of name decorator");

                break;

            case "parent":
                if (!isString(id))
                    return getError("Invalid or missing id parameter of parent decorator");

                break;

            // Ignore unknown decorators.
        }
        return null;
    }
    
    // Validate Entity
    public ResponseDto? validateEntity(){
        object? entity = getProperty(body.@params, "entity");
        object? kind = getProperty(entity, "kind");
        object? original = getProperty(entity, "original");

        if (!isObject(entity))
            return getError("Invalid or missing entity parameter");

        
        if (kind == null || !getStaticStrings(typeof(EntityKind)).Contains(kind.ToString()))
            return getError("Unsupported or missing entity kind parameter");

        if (!isObject(original))
            return getError("Invalid or missing entity original parameter");

        foreach (string n in getPropertyNames(original)){
            var err = validateDecorator(n, getProperty(original, n));
            if (err != null) return err;
        }

        object? name = getProperty(original, "name");
        if (name == null)
            return getError("Missing decorator", ErrorCode.MISSING_DECORATOR, name );

        object? language = getProperty(original, "language");
        if (language == null)
            return getError("Missing decorator", ErrorCode.MISSING_DECORATOR, language );

        return null;
    }

    //Validate binary contents
    public ResponseDto? validateBinaryContents() {
        object? binaryContents = getProperty(body.@params, "binaryContents");
        if (binaryContents == null || !isString(binaryContents))
            return getError("Invalid binary contents parameter");

        return null;
    }

    public ResponseDto? bodyMethod(){

        if (body.method == Method.ENTITY_GET)
            return validateXdip() ?? validateRequestParameters();
        if (body.method == Method.ENTITY_GET_BINARY)
            return validateXdip();
        if (body.method == Method.ENTITY_CREATE)
            return validateRequestParameters() ?? validateEntity() ?? validateBinaryContents();

        return null;
    }
}

