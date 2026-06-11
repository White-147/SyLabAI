namespace SyLabAI.Infrastructure.AI.Settings;

internal static class ProviderSettingsPathResolver
{
    public static string ResolveLocalSettingsPath()
    {
        var repoRoot = FindRepoRoot(Directory.GetCurrentDirectory())
            ?? FindRepoRoot(AppContext.BaseDirectory)
            ?? throw new InvalidOperationException("Unable to locate the SyLabAI repository root.");

        var configRoot = Path.Combine(repoRoot, ".config");
        Directory.CreateDirectory(configRoot);

        return Path.Combine(configRoot, "provider.local.json");
    }

    private static string? FindRepoRoot(string start)
    {
        var directory = new DirectoryInfo(start);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AGENTS.md"))
                && Directory.Exists(Path.Combine(directory.FullName, "apps"))
                && Directory.Exists(Path.Combine(directory.FullName, "backend")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
