using System.Text.Json;
namespace TDK_Boilerplate_C_.jsonrpc.response;

public class RequestDto
{
    public string jsonrpc { get; set; } 

    public string id { get; set; }

    public string method { get; set; }
    
    public JsonElement @params {get; set; }

}


