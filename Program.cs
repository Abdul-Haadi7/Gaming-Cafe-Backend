using System.Text;
using GAME_CAFE.Authorization;
using GAME_CAFE.Seeder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container
builder.Services.AddControllers();


// Register Database Seeder
builder.Services.AddScoped<DatabaseSeeder>();


// Authorization
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

builder.Services.AddAuthorization();


// API Explorer & Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Enter JWT token like: Bearer {your token}"
        });

    options.AddSecurityRequirement(
        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference =
                        new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                },

                Array.Empty<string>()
            }
        });
});


// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", corsBuilder =>
    {
        corsBuilder
            .WithOrigins(
                "http://localhost:4200",
                "http://localhost:3000",
                "http://localhost:8000"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });


    options.AddPolicy("ProdCors", corsBuilder =>
    {
        corsBuilder
            .WithOrigins("https://myProductionSite.com")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});


// JWT Configuration
string? tokenKeyString =
    builder.Configuration["AppSettings:TokenKey"];

SymmetricSecurityKey tokenKey =
    new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(tokenKeyString ?? "")
    );


TokenValidationParameters parameters =
    new TokenValidationParameters
    {
        IssuerSigningKey = tokenKey,

        ValidateIssuerSigningKey = true,

        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["AppSettings:Issuer"],

        ValidateAudience = true,
        ValidAudience = builder.Configuration["AppSettings:Audience"],

        ValidateLifetime = true
    };


builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = parameters;
    });


// Build application
var app = builder.Build();

//Seeder
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

    seeder.SeedSuperAdmin();
}

// Middleware Pipeline
app.UseCors("DevCors");
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();