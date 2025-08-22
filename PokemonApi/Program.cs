using SoapCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSoapCore();

var app = builder.Build();
app.Run();
