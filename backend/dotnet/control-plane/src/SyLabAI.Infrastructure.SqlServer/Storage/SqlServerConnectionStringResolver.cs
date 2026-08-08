using Microsoft.Extensions.Configuration;

namespace SyLabAI.Infrastructure.SqlServer.Storage;

internal static class SqlServerConnectionStringResolver
{
    public static string? Resolve(IConfiguration configuration)
    {
        var candidates = new[]
        {
            configuration.GetConnectionString("SyLabAI"),
            configuration["SyLabAI:SqlServer:ConnectionString"],
            configuration["SYLABAI_SQLSERVER_CONNECTION_STRING"],
            configuration["SYLABAI_SQL_SERVER_CONNECTION_STRING"]
        };

        return candidates.FirstOrDefault(candidate => !string.IsNullOrWhiteSpace(candidate));
    }
}
