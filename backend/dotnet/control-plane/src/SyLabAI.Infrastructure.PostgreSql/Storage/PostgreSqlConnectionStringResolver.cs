using Microsoft.Extensions.Configuration;

namespace SyLabAI.Infrastructure.PostgreSql.Storage;

internal static class PostgreSqlConnectionStringResolver
{
    public static string? Resolve(IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("SyLabAI")
            ?? configuration["SyLabAI:PostgreSql:ConnectionString"]
            ?? configuration["SYLABAI_POSTGRES_CONNECTION_STRING"];

        return string.IsNullOrWhiteSpace(connectionString)
            ? null
            : connectionString;
    }
}
