using SoapCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;              // <-- necesario
using Pomelo.EntityFrameworkCore.MySql;          // <-- necesario
using PokemonApi.Infrastructure;
using PokemonApi.Services;   
using PokemonApi.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSoapCore();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IPokemonRepository, PokemonRepository>();
builder.Services.AddScoped<IPokemonService, PokemonService>();
builder.Services.AddDbContext<RelationalDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));


var app = builder.Build();
app.UseSoapEndpoint<IPokemonService>("/PokemonService.svc", new SoapEncoderOptions());

app.Run();
