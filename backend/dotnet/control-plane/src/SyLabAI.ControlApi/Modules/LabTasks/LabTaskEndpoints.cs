using SyLabAI.Application.Tasks;
using SyLabAI.ControlApi.Contracts;

namespace SyLabAI.ControlApi.Modules.LabTasks;

internal static class LabTaskEndpoints
{
    public static IEndpointRouteBuilder MapLabTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/lab-tasks").WithTags("LabTasks");

        group.MapGet("/", async (ILabTaskService service, CancellationToken cancellationToken) =>
        {
            var tasks = await service.ListAsync(cancellationToken);
            return Results.Ok(tasks.Select(task => task.ToDto()).ToArray());
        });

        group.MapPost("/", async (
            CreateLabTaskDto request,
            ILabTaskService service,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.title))
            {
                return Results.BadRequest(new ValidationErrorDto("Task title is required."));
            }

            var task = await service.CreateAsync(
                new CreateLabTaskRequest(request.title, request.steps, request.reviewChecklist),
                cancellationToken);

            return Results.Created($"/api/lab-tasks/{task.Id}", task.ToDto());
        });

        return app;
    }
}

