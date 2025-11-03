namespace LAF.WebApi;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using LAF.DataAccess.Data;
using LAF.Services.Services;
using LAF.Service.Interfaces.Services;
using LAF.Service.Interfaces.Repositories;
using LAF.Services.Repositories;
using LAF.WebApi.Hubs;
using Microsoft.Identity.Web;
using LAF.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Get JWT settings from configuration
        var jwtKey = builder.Configuration["JwtSettings:SecretKey"];

        // Get Azure AD settings from configuration
        var azureAdClientId = builder.Configuration["AzureAd:ClientId"];
        var azureAdTenantId = builder.Configuration["AzureAd:TenantId"];

        // Add services to the container.
        builder.Services.AddDbContext<LAFDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Register repositories
        builder.Services.AddScoped<IRepoTradeRepository, RepoTradeRepository>();
        builder.Services.AddScoped<IFundRepository, FundRepository>();
        builder.Services.AddScoped<IRepoRateRepository, RepoRateRepository>();
        builder.Services.AddScoped<ICollateralTypeRepository, CollateralTypeRepository>();
        builder.Services.AddScoped<ICounterpartyRepository, CounterpartyRepository>();
        builder.Services.AddScoped<ICashAccountRepository, CashAccountRepository>();
        builder.Services.AddScoped<ICashflowRepository, CashflowRepository>();
        builder.Services.AddScoped<ISecurityRepository, SecurityRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ISignalRBroker, SignalRBroker>();

        // Register services
        builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
        builder.Services.AddScoped<IRepoTradeService, RepoTradeService>();
        builder.Services.AddScoped<ITargetCircleService, TargetCircleService>();
        builder.Services.AddScoped<ICashManagementService, CashManagementService>();
        builder.Services.AddScoped<ISecurityService, SecurityService>();
        // Dual Authentication - JWT and Azure AD
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer("JwtScheme", options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.ASCII.GetBytes(jwtKey ?? throw new InvalidOperationException("JWT secret key is not configured"))),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError("JWT Authentication failed: {Error}", context.Exception);

                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Add("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("JWT Token validated successfully");
                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("JWT Token received: {Token}", context.Token);
                    return Task.CompletedTask;
                }
            };
        })
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"), "AzureAdScheme");

        // Add Azure AD authentication if configured
        if (!string.IsNullOrEmpty(azureAdClientId) && !string.IsNullOrEmpty(azureAdTenantId))
        {
            // Microsoft.Identity.Web already configured the Azure AD scheme above
            builder.Services.Configure<JwtBearerOptions>("AzureAdScheme", options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogError("Azure AD Authentication failed: {Error}", context.Exception);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogInformation("Azure AD Token validated successfully");
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogInformation("Azure AD Token received: {Token}", context.Token);
                        return Task.CompletedTask;
                    }
                };
            });
        }

        // Add CORS - Allow specific origins for development with credentials
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.WithOrigins("http://localhost:4200", "http://localhost:4202", "http://localhost:4203")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials() // Allow credentials (cookies, authorization headers)
                      .WithExposedHeaders("Token-Expired") // Expose custom headers
                                                           // Add these methods explicitly for SignalR
                      .SetIsOriginAllowed(_ => true)
                      .AllowCredentials();
            });
        });

        builder.Services.AddControllers();

        // Add dual authorization policy
        builder.Services.AddAuthorization(options =>
        {
            // Default policy that accepts both JWT and Azure AD
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes("JwtScheme", "AzureAdScheme")
                .Build();

            // Explicit dual auth policy
            options.AddPolicy("DualAuth", policy =>
            {
                policy.RequireAuthenticatedUser()
                      .AddAuthenticationSchemes("JwtScheme", "AzureAdScheme");
            });
        });

        // Register the dual auth handler
        builder.Services.AddScoped<IAuthorizationHandler, DualAuthHandler>();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true; // Helps with debugging
        }); // Add SignalR with configuration

        // Configure Swagger
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "LAF API",
                Version = "v1",
                Description = "API for LAF application",
                Contact = new OpenApiContact
                {
                    Name = "LAF Team",
                    Email = "support@laf.com"
                }
            });

            // Add JWT Authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            // Add Azure AD Authentication
            options.AddSecurityDefinition("AzureAD", new OpenApiSecurityScheme
            {
                Description = "Azure AD JWT Bearer token. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "LAF API V1");
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                c.DefaultModelsExpandDepth(-1); // Hide schemas section by default
            });
        }

        app.UseHttpsRedirection();

        // Important: CORS middleware must come before SignalR and other middleware
        app.UseCors("AllowAll");

        // Authentication must come before Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHub<LafHub>("/hubs/laf"); // Map SignalR hub

        app.Run();
    }
}
