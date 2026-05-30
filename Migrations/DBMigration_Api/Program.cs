using DBMigration_Api.Extensions;
using Infrastructure.Database.Repositories;
using Infrastructure.Services.LogFile;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureDbContext(builder.Configuration);
builder.Services.AddSingleton<LogFileService>();
//builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
