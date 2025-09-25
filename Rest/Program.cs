using peliculaApi.Services;
using peliculaApi.Mappers;
using peliculaApi.Gateways;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddScoped<IPeliculaService, PeliculaService>();
builder.Services.AddScoped<IPeliculaGateway, PeliculaGateway>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();