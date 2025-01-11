using Microsoft.AspNetCore.Mvc;

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
                        [FromQuery] bool include_deleted = false
                    ) =>
                    {
                        var building = await service.GetAllBuildingsAsync(include_deleted);
                        return new { buildings = building };
                    }
                )
                .WithName("GetAllBuildings")
                .WithOpenApi()
                .WithTags("Buildings");

            app.MapGet(
                    "/api/v3/assets/buildings/{id}",
                    async ([FromServices] IBuildingService service, Guid id) =>
                    {
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
                    async ([FromServices] IBuildingService service, [FromBody] Building building) =>
                    {
                        // Überprüfen, ob das Gebäude gültig ist
                        if (building == null)
                        {
                            return Results.BadRequest("Building data is required.");
                        }

                        if (string.IsNullOrEmpty(building.name))
                        {
                            return Results.BadRequest(new { Message = "Name is required." });
                        }

                        if (string.IsNullOrEmpty(building.streetname))
                        {
                            return Results.BadRequest(new { Message = "Street name is required." });
                        }

                        if (string.IsNullOrEmpty(building.housenumber))
                        {
                            return Results.BadRequest(
                                new { Message = "House number is required." }
                            );
                        }

                        if (string.IsNullOrEmpty(building.country_code))
                        {
                            return Results.BadRequest(
                                new { Message = "Country code is required." }
                            );
                        }

                        if (string.IsNullOrEmpty(building.postalcode))
                        {
                            return Results.BadRequest(new { Message = "Postal code is required." });
                        }

                        if (string.IsNullOrEmpty(building.city))
                        {
                            return Results.BadRequest(new { Message = "City is required." });
                        }

                        // Gebäude erstellen
                        var createdBuilding = await service.CreateBuildingAsync(building);

                        // Erfolg und das neu erstellte Gebäude zurückgeben
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

            app.MapDelete(
                    "/api/v3/assets/buildings/{id}",
                    async (
                        [FromServices] IBuildingService service,
                        Guid id,
                        [FromQuery] bool permanent = false
                    ) =>
                    {
                        var (success, errorMessage) = await service.DeleteBuildingAsync(id, permanent);

                        if (success)
                            return Results.NoContent();

                        return Results.BadRequest(new { Error = errorMessage });
                    }
                )
                .WithName("DeleteBuilding")
                .WithOpenApi()
                .WithTags("Buildings")
                .RequireAuthorization();


            app.MapPut(
                    "/api/v3/assets/buildings/{id}",
                    async ([FromServices] IBuildingService service, Guid id, Building building) =>
                    {
                        // Explizite Dekonstruktion (kein `var`)
                        (Building updatedBuilding, bool isCreated) = await service.UpdateBuildingAsync(building, id);

                        if (isCreated)
                        {
                            return Results.Created($"/api/v3/assets/buildings/{updatedBuilding.id}", updatedBuilding); // 201 Created
                        }

                        return Results.Ok(updatedBuilding); // 200 OK
                    }
                )
                .WithName("UpdateBuilding")
                .WithOpenApi()
                .WithTags("Buildings")
                .RequireAuthorization();



        }
    }
}
