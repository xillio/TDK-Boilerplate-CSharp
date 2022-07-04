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
        
        // Define here which service to use
        var service = new FileService();
        
        Execute ex = new Execute(req, service);
        ResponseDto? res2 = ex.run();

        if (res2 != null) return res2;

        ResponseDto err = new ResponseDto{
            jsonrpc = ProtocolVersion.V2_0, 
            id = req.id,
            error = new ErrorDto{
                code = (int)ErrorCode.CONNECTOR_OPERATION_FAILED,
                message = "Controller failed to execute JSON RPC",
                data = new object()
            }
        };
        return err;
    }
}
