using Microsoft.AspNetCore.Mvc;
using TDK_Boilerplate_C_.jsonrpc;
using TDK_Boilerplate_C_.jsonrpc.response; 
using System;

namespace TDK_Boilerplate_C_.Controllers;


[ApiController]
[Route("sample-connector")]  // Base url of controller
public class Controller : ControllerBase
{

    [HttpPost] // End-point
    public ResponseDto Post(RequestDto req)
    {
        Validate val = new Validate(req);
        ResponseDto? res = val.doValidate();

        if (res != null) return res;
        
        //Console.WriteLine(req.jsonrpc);

        ResponseDto err = new ResponseDto{
            jsonrpc = ProtocolVersion.V2_0, 
            id = "6",
            error = new ErrorDto{
                code = 404,
                message = "test error response ",
                data = new object()
            }
        };
        return err;
    }
}
