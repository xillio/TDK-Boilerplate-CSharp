using Microsoft.AspNetCore.Mvc;
using TDK_Boilerplate_C_.jsonrpc; 
using System;

namespace TDK_Boilerplate_C_.Controllers;


[ApiController]
[Route("sample-connector")]  // Base url of controller
public class Controller : ControllerBase
{

    [HttpPost] // End-point
    public ResponseDto Post(RequestDto req)
    {

        Console.WriteLine(req.jsonrpc);
        return new ErrorDto
        {
            jsonrpc = "test",
            id = "6",
            code = 404,
            message = "yeet yeet lick some feet",
            data = new object()
        } ;
    }
}
