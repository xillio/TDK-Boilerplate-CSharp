using Microsoft.AspNetCore.Mvc;
using TDK_Boilerplate_C_.jsonrpc;
using TDK_Boilerplate_C_.jsonrpc.response; 
using TDK_Boilerplate_C_.Service;
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
        
        Execute ex = new Execute(req, new FileService());
        ResponseDto? res2 = ex.run();

        if (res2 != null) return res2;

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
