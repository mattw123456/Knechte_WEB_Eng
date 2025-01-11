using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebEngineering.Buildings;

namespace WebEngineering.Storey
{
    public static class StoreyEndpunkte
    {
        public static void AddStoreyEndpunkte(this IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "/api/v3/assets/storeys",
                    async (
                        [FromServices] IStoreyService service,
                        [FromServices] ILogger<IStoreyService> logger,
                        [FromQuery] Guid? building_id = null,
                        [FromQuery] bool include_deleted = false
                    ) =>
                    {
                        logger.LogInformation(
                            "Fetching all storeys (building_id={building_id}, include_deleted={include_deleted})",
                            building_id,
                            include_deleted
                        );

                        var storeys = await service.GetAllStoreysAsync(building_id, include_deleted);
                        return new { storeys = storeys };
                    }
                )
                .WithName("GetAllStoreys")
                .WithOpenApi()
                .WithTags("Storey");

            app.MapPost(
                    "/api/v3/assets/storeys",
                    async (
                        [FromServices] IStoreyService service,
                        [FromServices] IBuildingService buildingService,
                        [FromServices] ILogger<IStoreyService> logger,
                        [FromBody] Storey storey
                    ) =>
                    {
                        logger.LogInformation("Creating a new storey with data: {@storey}", storey);

                        var building = await buildingService.GetBuildingByIdAsync(storey.building_id);

                        if (building == null || building.deleted_at != null)
                        {
                            logger.LogWarning(
                                "Failed to create storey: building_id={building_id} not found or deleted",
                                storey.building_id
                            );
                            return Results.BadRequest(
                                new { Message = "Building not found or deleted." }
                            );
                        }

                        var createdStorey = await service.CreateStoreyAsync(storey);

                        logger.LogInformation(
                            "Storey successfully created with ID={createdStoreyId}",
                            createdStorey.id
                        );

                        return Results.Created(
                            $"/api/v3/assets/storeys/{createdStorey.id}",
                            createdStorey
                        );
                    }
                )
                .WithName("CreateStorey")
                .WithOpenApi()
                .WithTags("Storey")
                .RequireAuthorization();

            app.MapGet(
                    "/api/v3/assets/storeys/{id}",
                    async (
                        [FromServices] IStoreyService service,
                        [FromServices] ILogger<IStoreyService> logger,
                        Guid id
                    ) =>
                    {
                        logger.LogInformation("Fetching storey details for ID={id}", id);

                        var storey = await service.GetStoreyByIdAsync(id);

                        if (storey == null)
                        {
                            logger.LogWarning("Storey with ID={id} not found", id);
                            return Results.NotFound(new { Message = "Storey not found." });
                        }

                        logger.LogInformation("Storey details fetched for ID={id}", id);
                        return Results.Ok(storey);
                    }
                )
                .WithName("GetStoreyById")
                .WithOpenApi()
                .WithTags("Storey");

            app.MapPut(
                    "/api/v3/assets/storeys/{id}",
                    async (
                        [FromServices] IStoreyService storeyService,
                        [FromServices] IBuildingService buildingService,
                        [FromServices] ILogger<IStoreyService> logger,
                        Guid id,
                        Storey storey
                    ) =>
                    {
                        logger.LogInformation("Updating storey with ID={id} and data: {@storey}", id, storey);

                        if (storey == null)
                            return Results.BadRequest("Storey data is required.");

                        var building = await buildingService.GetBuildingByIdAsync(storey.building_id);

                        if (building == null || building.deleted_at != null)
                        {
                            logger.LogWarning(
                                "Failed to update storey: building_id={building_id} not found or deleted",
                                storey.building_id
                            );
                            return Results.BadRequest("Building not found or deleted.");
                        }

                        if (storey.deleted_at != null)
                        {
                            logger.LogWarning("Failed to update storey: Storey ID={id} is deleted", id);
                            return Results.BadRequest("Storey is deleted and cannot be modified");
                        }

                        var updatedStorey = await storeyService.UpdateStoreyAsync(storey, id);

                        if (updatedStorey == null)
                        {
                            logger.LogWarning("Storey with ID={id} not found for update", id);
                            return Results.NotFound("Storey not found or could not be updated.");
                        }

                        logger.LogInformation("Storey with ID={id} successfully updated", id);
                        return Results.Ok(updatedStorey);
                    }
                )
                .WithName("UpdateStorey")
                .WithOpenApi()
                .WithTags("Storey")
                .RequireAuthorization();

            app.MapDelete(
                    "/api/v3/assets/storeys/{id}",
                    async (
                        [FromServices] IStoreyService service,
                        [FromServices] ILogger<IStoreyService> logger,
                        Guid id,
                        [FromQuery] bool permanent = false
                    ) =>
                    {
                        logger.LogInformation("Deleting storey with ID={id} (permanent={permanent})", id, permanent);

                        var deletedSuccessfully = await service.DeleteStoreyAsync(id, permanent);

                        if (deletedSuccessfully)
                        {
                            logger.LogInformation("Storey with ID={id} successfully deleted", id);
                            return Results.NoContent();
                        }

                        logger.LogWarning("Failed to delete storey with ID={id}", id);
                        return Results.BadRequest();
                    }
                )
                .WithName("DeleteStorey")
                .WithOpenApi()
                .WithTags("Storey")
                .RequireAuthorization();
        }
    }
}
