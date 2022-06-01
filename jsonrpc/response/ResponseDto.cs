namespace TDK_Boilerplate_C_.jsonrpc.response;

public class ResponseDto
{
    public string jsonrpc { get; set; } 

    public string id { get; set; }

    public ErrorDto? error { get; set; }

    public ResultDto? result { get; set; }

}


