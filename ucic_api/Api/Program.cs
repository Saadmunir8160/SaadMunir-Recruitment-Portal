using Application;
using Application.Common.Interfaces;
using Infra.Services;
using Infra;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Serilog;
using Application.Mappings;
using System.Security.Claims;
using Infra.Identity.Seeding;
using Application.Commands.Auth;
using Application.DTOs;
using MediatR;
using Microsoft.Extensions.FileProviders;
using Application.Common.Extensions;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "UCIC API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });

    // Handle potential issues with complex types
    c.CustomSchemaIds(type => type.FullName);
});

// Configure JSON serialization to handle potential circular references
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();

// Configure CORS
builder.Services.Configure<Application.Common.Configurations.CorsSettings>(
    builder.Configuration.GetSection("CorsSettings"));

var corsSettings = builder.Configuration.GetSection("CorsSettings").Get<Application.Common.Configurations.CorsSettings>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DealerPortalPolicy", corsBuilder =>
    {
        var corsConfig = corsBuilder.WithOrigins(corsSettings?.AllowedOrigins ?? new string[] { });
        
        if (corsSettings?.AllowCredentials == true)
        {
            corsConfig.AllowCredentials();
        }
        
        corsConfig
            .AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowedToAllowWildcardSubdomains();

        // Add exposed headers if configured
        if (corsSettings?.ExposedHeaders?.Length > 0)
        {
            corsConfig.WithExposedHeaders(corsSettings.ExposedHeaders);
        }
    });

    // For development - more permissive policy
    options.AddPolicy("DevelopmentPolicy", corsBuilder =>
    {
        corsBuilder
            .SetIsOriginAllowed(origin => 
            {
                // Allow localhost with any port
                if (string.IsNullOrEmpty(origin)) return false;
                var uri = new Uri(origin);
                return uri.Host == "localhost" || 
                       uri.Host == "127.0.0.1" || 
                       uri.Host.EndsWith(".ucic.com") ||
                       uri.Host.EndsWith(".unitedcement.com.sa");
            })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });

    // Default fallback policy
    options.AddDefaultPolicy(corsBuilder =>
    {
        corsBuilder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Add API Configuration
builder.Services.Configure<Application.Common.Configurations.ApiConfiguration>(
builder.Configuration.GetSection("ApiConfiguration"));

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddExternalApiService();

// Register database seeders
builder.Services.AddScoped<DataSeeder>();
builder.Services.AddScoped<DatabaseCleanupSeeder>();

var _key = builder.Configuration["Jwt:Key"];
var _issuer = builder.Configuration["Jwt:Issuer"];
var _audience = builder.Configuration["Jwt:Audience"];
var _expirtyMinutes = builder.Configuration["Jwt:ExpiryMinutes"];

builder.Services.AddSingleton<ITokenGenerator>(new TokenGenerator(_key!, _issuer!, _audience!, _expirtyMinutes!));



// Configuration for token
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = _audience,
        ValidIssuer = _issuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
        ClockSkew = TimeSpan.FromMinutes(Convert.ToDouble(_expirtyMinutes))

    };
});

builder.Host.UseSerilog();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure error handling for development
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // Use development CORS policy for development environment
    app.UseCors("DevelopmentPolicy");
}
else
{
    // Use stricter CORS policy for production
    app.UseCors("DealerPortalPolicy");
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "UCIC API v1");
    c.RoutePrefix = "swagger"; // Changed from string.Empty to "swagger"
    c.DefaultModelsExpandDepth(-1); // Hide models section
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    c.EnableFilter();
    c.EnableDeepLinking();
    c.DisplayRequestDuration();
    c.EnableTryItOutByDefault();
});

app.UseHttpsRedirection();
app.UseRouting();

// CORS middleware should be placed after UseRouting and before UseAuthentication
// This is already handled above based on environment

app.UseAuthentication();
app.UseAuthorization();

var staticPaths = new[] { "News", "Payment", "Vendors", "Product", "Resume" };
foreach (var path in staticPaths)
{
    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", path);
    if (!Directory.Exists(fullPath))
    {
        Directory.CreateDirectory(fullPath);
    }
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(fullPath),
        RequestPath = $"/static/{path}"
    });
}
app.UseMiddleware<Api.Middlewares.CustomExceptionMiddleware>();
app.MapControllers();
app.UseStaticFiles();

//to seed admin user and roles automaticaly
_ = Task.Run(async () =>
{
    try
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        // Log seeding errors but don't block application startup
        Console.WriteLine($"Database seeding failed: {ex.Message}");
    }
});

app.Run();

public partial class Program { }
