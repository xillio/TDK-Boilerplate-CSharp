/*When a RPC call encounters an error, the response object must contain the error member with a value that is an object with the following members:
code - A Number that indicates the error type that occurred. This must be an integer.
message - A String providing a short description of the error.
data - A Primitive or Structured value that contains additional information about the error. This may be omitted.*/
using TDK_Boilerplate_C_.jsonrpc.response;

namespace TDK_Boilerplate_C_.jsonrpc.response;

public class ErrorDto
{
    public int code { get; set; } 

    public string message { get; set; }

    public object? data { get; set; }

}