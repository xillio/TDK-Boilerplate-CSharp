using Microsoft.AspNetCore.Diagnostics;
using TDK_Boilerplate_C_.jsonrpc.response; 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
