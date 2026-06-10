// Ignore Spelling: Cors
using Infrastructure.Database.Context;
using Infrastructure.Services.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using static Infrastructure.Database.Context.StoreContext;


namespace Store_API.Extenstions;

public static class ServicesExtensions
{
    public static void ConfigureCors(this IServiceCollection services, IConfiguration configuration) =>
     services.AddCors(options =>
     {
         var originsConfig = configuration.GetSection("AllowedCors").Value;
         var originsArr = originsConfig?.Split(',', StringSplitOptions.RemoveEmptyEntries);

         if (!string.IsNullOrWhiteSpace(originsConfig) && originsConfig != "*" && (originsArr?.Any() ?? false))
         {
             options.AddPolicy("AllowSpecificOrigin",
                 builder => builder.WithOrigins(originsArr) // Must be explicitly set
                 .AllowAnyHeader()
                 .AllowAnyMethod()
                 .AllowCredentials()
                 .SetIsOriginAllowed(_ => true));
             //.SetIsOriginAllowed(origin =>
             //       string.IsNullOrEmpty(origin) || originsArr.Contains(origin)));

         }
         else
         {
             options.AddPolicy("AllowSpecificOrigin", builder =>
             {
                 builder.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                     .SetIsOriginAllowed(_ => true);
             });
         }
     });

    public static void ConfigureDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        var connectionString = configuration.GetConnectionString("Store")
            ?? throw new InvalidOperationException("Connection string 'Store' is not configured in appsettings.json.");

        //services.AddScoped<DispatchDomainEventsInterceptor>();
        services.AddScoped<GetCurrentUserId>(sp =>
        {
            return () =>
            {
                var userId = sp.GetService<IHttpContextAccessor>()?.HttpContext?.User?.Claims
                    .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
                return long.TryParse(userId, out long res) ? res : (long?)null;
            };
        });

        services.AddDbContext<StoreContext>((serviceProvider, options) =>
        {
            options.UseSqlServer(connectionString,
                sqlServerOptionsAction: sqloption =>
                {
                    sqloption.MigrationsAssembly("DBMigration");
                    sqloption.EnableRetryOnFailure();
                });

        });
    }





    public static void ConfigureSwaggerGen(this IServiceCollection services) =>
           services.AddSwaggerGen(c =>
           {
               c.SwaggerDoc("v1", new OpenApiInfo { Title = "Store_API", Version = "v1" });
               #region JWT Token
               c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
               {
                   Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
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

               #endregion
           });
}
