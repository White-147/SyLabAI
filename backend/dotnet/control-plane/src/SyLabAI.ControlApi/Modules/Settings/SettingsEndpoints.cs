using SyLabAI.Application.Settings;
using SyLabAI.ControlApi.Contracts;

namespace SyLabAI.ControlApi.Modules.Settings;

internal static class SettingsEndpoints
{
    public static IEndpointRouteBuilder MapSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/settings").WithTags("Settings");

        group.MapGet("/provider", (IProviderStatusService service) =>
        {
            var status = service.GetStatus();
            return Results.Ok(new ProviderStatusDto(
                status.Provider,
                status.Model,
                status.Configured,
                status.Mode,
                status.SafetyGates));
        });

        return app;
    }
}

