// Ignore Spelling: Ap
using Infrastructure.Database.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DBMigration_Api.Extensions;

public static class ServicesExtensions
{
    public static void ConfigureDbContext(this IServiceCollection services, IConfiguration configuration) =>
        services.AddDbContext<StoreContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("Store"),
            sqlServerOptionsAction: sqloption =>
            {
                sqloption.MigrationsAssembly("DBMigration");
            });
        });


}
