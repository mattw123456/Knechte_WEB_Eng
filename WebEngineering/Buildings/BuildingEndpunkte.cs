using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebEngineering.Buildings
{
    public static class BuildingEndpunkte
    {
        public static void AddBuildingEndpunkte(this IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "/api/v3/assets/buildings",
                    async (
                        [FromServices] IBuildingService service,
                        ILogger<BuildingService> logger,
                        [FromQuery] bool include_deleted = false
                    ) =>
                    {
                        logger.LogInformation("Fetching all buildings (include_deleted={include_deleted})", include_deleted);

                        var buildings = await service.GetAllBuildingsAsync(include_deleted);
                        return new { buildings };
                    }
                )
                .WithName("GetAllBuildings")
                .WithOpenApi()
                .WithTags("Buildings");

            app.MapGet(
                    "/api/v3/assets/buildings/{id}",
                    async ([FromServices] IBuildingService service, Guid id, ILogger<BuildingService> logger) =>
                    {
                        logger.LogInformation("Fetching building with ID {id}", id);

                        var building = await service.GetBuildingByIdAsync(id);
                        return building is not null
                            ? Results.Ok(building)
                            : Results.NotFound(new { Message = "Building not found." });
                    }
                )
                .WithName("GetBuildingById")
                .WithOpenApi()
                .WithTags("Buildings");

            app.MapPost(
                    "/api/v3/assets/buildings",
                    async (
                        [FromServices] IBuildingService service,
                        [FromBody] Building building,
                        ClaimsPrincipal user,
                        ILogger<BuildingService> logger
                    ) =>
                    {
                        var userId = user.Claims.FirstOrDefault(c => c.Type == "sid")?.Value;
                        logger.LogInformation("User {userId} creates a building: {@building}", userId, building);

                        if (building == null)
                            return Results.BadRequest("Building data is required.");

                        if (string.IsNullOrEmpty(building.name))
                            return Results.BadRequest(new { Message = "Name is required." });

                        // Weitere Validierungen können hier hinzugefügt werden...

                        var createdBuilding = await service.CreateBuildingAsync(building);

                        logger.LogInformation("Building created with ID {id} by user {userId}", createdBuilding.id, userId);

                        return Results.Created(
                            $"/api/v3/assets/buildings/{createdBuilding.id}",
                            createdBuilding
                        );
                    }
                )
                .WithName("CreateBuilding")
                .WithOpenApi()
                .WithTags("Buildings")
                .RequireAuthorization();

            app.MapPut(
                    "/api/v3/assets/buildings/{id}",
                    async (
                        [FromServices] IBuildingService service,
                        Guid id,
                        [FromBody] Building building,
                        ClaimsPrincipal user,
                        ILogger<BuildingService> logger
                    ) =>
                    {
                        var userId = user.Claims.FirstOrDefault(c => c.Type == "sid")?.Value;
                        logger.LogInformation("User {userId} updates building with ID {id}: {@building}", userId, id, building);

                        var (updatedBuilding, isCreated) = await service.UpdateBuildingAsync(building, id);

                        if (isCreated)
                        {
                            logger.LogInformation("Building with ID {id} was created by user {userId}", id, userId);
                            return Results.Created($"/api/v3/assets/buildings/{updatedBuilding.id}", updatedBuilding);
                        }

                        logger.LogInformation("Building with ID {id} was updated by user {userId}", id, userId);
                        return Results.Ok(updatedBuilding);
                    }
                )
                .WithName("UpdateBuilding")
                .WithOpenApi()
                .WithTags("Buildings")
                .RequireAuthorization();

            app.MapDelete(
                    "/api/v3/assets/buildings/{id}",
                    async (
                        [FromServices] IBuildingService service,
                        Guid id,
                        ClaimsPrincipal user,
                        ILogger<BuildingService> logger,
                        [FromQuery] bool permanent = false
                    ) =>
                    {
                        var userId = user.Claims.FirstOrDefault(c => c.Type == "sid")?.Value;
                        logger.LogInformation("User {userId} deletes building with ID {id} (permanent={permanent})", userId, id, permanent);

                        var (success, errorMessage) = await service.DeleteBuildingAsync(id, permanent);

                        if (success)
                        {
                            logger.LogInformation("Building with ID {id} successfully deleted by user {userId}", id, userId);
                            return Results.NoContent();
                        }

                        logger.LogWarning("Failed to delete building with ID {id} by user {userId}: {errorMessage}", id, userId, errorMessage);
                        return Results.BadRequest(new { Error = errorMessage });
                    }
                )
                .WithName("DeleteBuilding")
                .WithOpenApi()
                .WithTags("Buildings")
                .RequireAuthorization();
        }
    }
}
