using Microsoft.AspNetCore.Diagnostics;
using TDK_Boilerplate_C_.jsonrpc.response; 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

/*app.UseExceptionHandler(c => c.Run(async context =>
{
    var exception = context.Features
        .Get<IExceptionHandlerPathFeature>()
        ?.Error;

    ResponseDto err = new ResponseDto{
            jsonrpc = ProtocolVersion.V2_0, 
            id = "6",
            error = new ErrorDto{
                code = 404,
                message = "test error response ",
                data = new object()
            }
        };

    Console.WriteLine("lol error");

    await context.Response.WriteAsJsonAsync(err);
    //var response = new { error = exception?.Message ?? "" };
    //await context.Response.WriteAsJsonAsync(response);
}));*/

app.Run();
