using SyLabAI.Application.Settings;
using SyLabAI.ControlApi.Contracts;

namespace SyLabAI.ControlApi.Modules.Settings;

internal static class SettingsEndpoints
{
    public static IEndpointRouteBuilder MapSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/settings").WithTags("Settings");

        group.MapGet("/provider", (IProviderSettingsService service) =>
        {
            return Results.Ok(service.GetStatus().ToDto());
        });

        group.MapPut("/provider", (
            UpdateProviderSettingsDto request,
            IProviderSettingsService service) =>
        {
            var validation = Validate(request);
            if (validation is not null)
            {
                return Results.BadRequest(validation);
            }

            var status = service.SaveSettings(new ProviderSettingsUpdate(
                request.baseUrl,
                request.model,
                request.apiKey,
                request.liveCallsEnabled));

            return Results.Ok(status.ToDto());
        });

        group.MapGet("/provider/models", async (
            IProviderSettingsService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.ListModelsAsync(cancellationToken);
            return Results.Ok(result.ToDto());
        });

        group.MapDelete("/provider/api-key", (IProviderSettingsService service) =>
        {
            return Results.Ok(service.ClearApiKey().ToDto());
        });

        group.MapPost("/provider/connectivity-tests", async (
            IProviderSettingsService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.TestConnectivityAsync(cancellationToken);
            return Results.Ok(result.ToDto());
        });

        return app;
    }

    private static ValidationErrorDto? Validate(UpdateProviderSettingsDto request)
    {
        if (!Uri.TryCreate(request.baseUrl, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
        {
            return new ValidationErrorDto("Provider Base URL must be an HTTPS absolute URL.");
        }

        return null;
    }
}
