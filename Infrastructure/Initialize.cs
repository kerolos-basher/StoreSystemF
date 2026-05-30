
using Application.Abstractions.Persistence;
using Application.Abstractions.Services;
using Application.UOW;
using Infrastructure.Database.Context;
using Infrastructure.Database.Repositories.Generic_Repository;
using Infrastructure.Services.QRCode;
using Infrastructure.Services.Sequences;
using Infrastructure.UOW;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class Initialize
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {

        services.AddScoped<IStoreUOW, StoreUOW>();
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<StoreContext>());
        services.AddScoped<ISequenceService, SequenceService>();
        services.AddScoped<IQRCodeService, QRCodeService>();
        services.AddSingleton<LogFileService>();
        services.AddScoped(typeof(ILookupRepository<>), typeof(LookupRepository<>));


        return services;
    }
}
