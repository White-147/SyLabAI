using Microsoft.Extensions.Configuration;

namespace SyLabAI.Infrastructure.Sqlite.Storage;

/// <summary>
/// SQLite 连接串解析：支持 配置 → 环境变量 两级覆盖。
/// 配置优先级：SyLabAI:Sqlite:ConnectionString → SYLABAI_SQLITE_CONNECTION_STRING。
/// 未配置时默认 Data Source=storage/sylabai.db（相对应用工作目录）。
/// </summary>
internal static class SqliteConnectionStringResolver
{
    public static string Resolve(IConfiguration configuration)
    {
        var configured = configuration["SyLabAI:Sqlite:ConnectionString"];
        var environment = Environment.GetEnvironmentVariable("SYLABAI_SQLITE_CONNECTION_STRING");

        if (!string.IsNullOrWhiteSpace(environment))
        {
            return environment;
        }

        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        return "Data Source=storage/sylabai.db";
    }
}
