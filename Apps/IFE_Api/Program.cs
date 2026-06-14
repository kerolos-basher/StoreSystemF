using Application;
using AutoMapper;
using Store_API;
using Store_API.Extenstions;
using Infrastructure;
using Infrastructure.Database.Context;
using Infrastructure.Services.Authentication;
using MediatR;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.FileProviders;
using System.Globalization;
using System.Text.Json;
using Utilities.Config;
using Utilities.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("clientconfigsettings.json", optional: true, reloadOnChange: true);

builder.Services.Configure<ClientConfigSettings>(builder.Configuration.GetSection("ClientConfigSettings"));

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Converters.Add(new LocalDateTimeConverter());
        options.JsonSerializerOptions.Converters.Add(new NullableLocalDateTimeConverter());
    });
builder.Services.AddAutoMapper(cfg => { }, typeof(Program));
builder.Services.AddApplicationServices();

#region AuthConfig
var authConfig = builder.Configuration.GetSection("AuThConfig").Get<AuThConfiguration>();

if (authConfig != null)
{
    builder.Services.AddSingleton(authConfig);
}
#endregion

builder.Services
    .AddInfrastructureServices()
    .AddApiServices(builder.Configuration);



#region Resource
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
                new CultureInfo("en-US"),
                new CultureInfo("ar-EG")
     };
    options.DefaultRequestCulture = new RequestCulture("ar-EG");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});
#endregion

var app = builder.Build();
app.UseApiServices();
if (app.Environment.IsDevelopment())
{
    //await app.InitialiseDatabaseAsync();

}

app.Run();
