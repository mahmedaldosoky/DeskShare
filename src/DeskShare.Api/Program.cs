using DeskShare.Api;
using DeskShare.Api.Authentication;
using DeskShare.Application;
using DeskShare.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DeskShare")
    ?? throw new InvalidOperationException("Connection string 'DeskShare' is missing.");

builder.Services
    .AddApplication()
    .AddInfrastructure(connectionString)
    .AddDeskShareAuthentication(builder.Configuration, builder.Environment)
    .AddApi();

var app = builder.Build();

await app.Services.InitializeDatabaseAsync(seedSampleData: app.Environment.IsDevelopment());

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program;
