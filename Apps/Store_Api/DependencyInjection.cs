
using Hangfire;
using Store_API.Extenstions;
using Store_API.Filters;
using Microsoft.Extensions.FileProviders;

namespace Store_API;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<ValidationExceptionFilter>();
            options.Filters.Add<DomainExceptionFilter>();
        });
        services.AddEndpointsApiExplorer();
        services.ConfigureSwaggerGen();
        services.ConfigureDbContext(configuration);
        services.ConfigureCors(configuration);

        return services;
    }

    public static WebApplication UseApiServices(this WebApplication app)
    {

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();
        app.UseExceptionHandler(options => { });

        app.UseRouting();
        app.UseCors("AllowSpecificOrigin");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();


        return app;
    }
}
