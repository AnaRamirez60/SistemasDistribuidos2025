using SoapCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;        
using PeliculaApi.Infrastructure;
using PeliculaApi.Services;   
using PeliculaApi.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSoapCore();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IPeliculaRepository, PeliculaRepository>();
builder.Services.AddScoped<IPeliculaService, PeliculaService>();
builder.Services.AddDbContext<RelationalDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));


var app = builder.Build();
app.UseSoapEndpoint<IPeliculaService>("/PeliculaService.svc", new SoapEncoderOptions());

app.Run();
