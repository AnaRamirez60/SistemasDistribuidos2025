using PokedexApi.Services;
using PokedexApi.Mappers;
using PokedexApi.Gateways;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddScoped<IPokemonService, PokemonService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => 
            {
            options.Authority = builder.Configuration.GetValue<string>("Authentication:Authority");
            options.TokenValidationParameters = new TokenValidationParameters 
            {
                ValidateIssuer = false,
                ValidIssuer = builder.Configuration.GetValue<string>("Authentication:Issuer"),
                ValidateActor = false,
                ValidateLifetime = false,
                ValidateAudience = false,
                ValidAudience = "pokedex-api",
                ValidateIssuerSigningKey = true
            };
            options.RequireHttpsMetadata = false;
            });

builder.Services.AddAuthorization(options => 
        {
        options.AddPolicy("Read", policy => policy.RequireClaim("http://schemas.microsoft.com/identity/claims/scope", 
                    "read"));
        options.AddPolicy("Write", policy => policy.RequireClaim("http://schemas.microsoft.com/identity/claims/scope", 
                    "write"));
        });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();