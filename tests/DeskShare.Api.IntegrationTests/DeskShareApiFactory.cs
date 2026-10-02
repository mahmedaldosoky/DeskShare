using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace DeskShare.Api.IntegrationTests;

public sealed class DeskShareApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"deskshare-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:DeskShare", $"Data Source={_databasePath}");
    }

    public async Task<HttpClient> CreateSignedInClientAsync(string name, bool isOfficeManager = false)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/dev-login", new
        {
            DisplayName = name,
            Email = $"{name.ToLowerInvariant()}@contoso.com",
            IsOfficeManager = isOfficeManager,
        });
        response.EnsureSuccessStatusCode();
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SqliteConnection.ClearAllPools();
        File.Delete(_databasePath);
    }
}
