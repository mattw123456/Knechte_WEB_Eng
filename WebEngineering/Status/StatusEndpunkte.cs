using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace WebEngineering.Status
{
    public static class StatusEndpunkte
    {
        public static void AddStatusEndpunkte(this IEndpointRouteBuilder app)
        {
            app.MapGet(
                "/api/v3/assets/status",
                ([FromServices] IStatusService service) =>
                {
                    return service.GebeStatusInformationenZurueck();
                }
            )
            .WithName("GetStatus")
            .WithOpenApi()
            .WithTags("Status");

            app.MapGet("/api/v3/assets/health", async (IStatusService statusService) =>
            {
                var healthInfo = statusService.GebeHealthInformationenZurueck();

                if (healthInfo.ready)
                {
                    return Results.Ok(healthInfo);
                }
                else
                {
                    return Results.Problem(
                        title: "Service Unavailable",
                        detail: "Database connection could not be established.",
                        statusCode: 503);
                }
            }
            )
            .WithName("HealthCheck")
            .WithOpenApi()
            .WithTags("Status");

            app.MapGet("/api/v3/assets/health/live", async (IStatusService statusService) =>
            {
                var liveinfo = statusService.GebeHealthInformationenZurueck();

                if (liveinfo.live)
                {
                    return Results.Ok("live: " + liveinfo.live);
                }
                else
                {
                    return Results.BadRequest("the service cannot be considered as live and should be replaced or restarted.");
                }
            }
            )
            .WithName("LiveCheck")
            .WithOpenApi()
            .WithTags("Status");

            app.MapGet("/api/v3/assets/health/ready", async (IStatusService statusService) =>
            {
                var readyinfo = statusService.GebeHealthInformationenZurueck();

                if (readyinfo.ready)
                {
                    return Results.Ok("ready: " + readyinfo.ready);
                }
                else
                {
                    return Results.BadRequest("the service cannot be considered as ready and should be replaced or restarted.");
                }
            }
            )
            .WithName("ReadyCheck")
            .WithOpenApi()
            .WithTags("Status");

        }
    }
}
