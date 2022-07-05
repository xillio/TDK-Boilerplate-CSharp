using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json.Serialization;
using TDK_Boilerplate_C_.jsonrpc.response; 
using System.Net.Mime;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options => {
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Keys
            .SelectMany(key => context.ModelState[key].Errors.Select(x => $"{key}: {x.ErrorMessage}"))
            .ToArray();

        ResponseDto err = new ResponseDto{
            jsonrpc = ProtocolVersion.V2_0, 
            id = "",
            error = new ErrorDto{
                code = (int)ErrorCode.INVALID_CONFIGURATION,
                message = errors[0],
                data = new object()
            }
        };

        var result = new ObjectResult(err);
        result.ContentTypes.Add(MediaTypeNames.Application.Json);

        return result;
    };
});

builder.Services.AddMvc().AddJsonOptions(options => {
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

var app = builder.Build();
app.MapControllers();
app.Run();
