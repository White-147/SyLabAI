using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SyLabAI.Infrastructure.AI.Settings;

#pragma warning disable CA1416

internal sealed class LocalProviderSettingsStore
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("SyLabAI.Provider.ApiKey.v1");
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly object _gate = new();
    private readonly string _settingsPath = ProviderSettingsPathResolver.ResolveLocalSettingsPath();

    public LocalProviderSettings Read()
    {
        lock (_gate)
        {
            if (!File.Exists(_settingsPath))
            {
                return new LocalProviderSettings();
            }

            var content = File.ReadAllText(_settingsPath, Encoding.UTF8);
            return JsonSerializer.Deserialize<LocalProviderSettings>(content, JsonOptions) ?? new LocalProviderSettings();
        }
    }

    public LocalProviderSettings Save(LocalProviderSettings settings)
    {
        var next = settings with { UpdatedAt = DateTimeOffset.UtcNow };

        lock (_gate)
        {
            var content = JsonSerializer.Serialize(next, JsonOptions);
            File.WriteAllText(_settingsPath, content, Encoding.UTF8);
        }

        return next;
    }

    public string ProtectApiKey(string apiKey)
    {
        var protectedBytes = ProtectedData.Protect(
            Encoding.UTF8.GetBytes(apiKey),
            Entropy,
            DataProtectionScope.CurrentUser);

        return Convert.ToBase64String(protectedBytes);
    }

    public string? UnprotectApiKey(LocalProviderSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.ProtectedApiKey))
        {
            return null;
        }

        try
        {
            var bytes = Convert.FromBase64String(settings.ProtectedApiKey);
            var unprotectedBytes = ProtectedData.Unprotect(bytes, Entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(unprotectedBytes);
        }
        catch (CryptographicException)
        {
            return null;
        }
        catch (FormatException)
        {
            return null;
        }
    }
}

internal sealed record LocalProviderSettings(
    string? BaseUrl = null,
    string? Model = null,
    bool? LiveCallsEnabled = null,
    string? ProtectedApiKey = null,
    DateTimeOffset? UpdatedAt = null);

#pragma warning restore CA1416
