using Microsoft.AspNetCore.Mvc;
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
                        [FromQuery] Guid? building_id = null,
                        [FromQuery] bool include_deleted = false
                    ) =>
                    {
                        var storey = await service.GetAllStoreysAsync(building_id, include_deleted);
                        return new { storeys = storey };
                    }
                )
                .WithName("GetAllStoreys")
                .WithOpenApi()
                .WithTags("Storey");

            app.MapPost(
                    "/api/v3/assets/storeys",
                    async (
                        [FromServices] IStoreyService service,
                        [FromServices] IBuildingService buildingservice,
                        [FromBody] Storey storey
                    ) =>
                    {
                        // Überprüfen, ob das Gebäude existiert und nicht gelöscht ist
                        var building = await buildingservice.GetBuildingByIdAsync(
                            storey.building_id
                        );

                        if (building == null || building.deleted_at != null)
                        {
                            // Fehler, wenn das Gebäude nicht existiert oder gelöscht ist
                            return Results.BadRequest(
                                new { Message = "Building not found or deleted." }
                            );
                        }

                        // Storey mit der Gebäude-ID verknüpfen
                        storey.building_id = storey.building_id;

                        // Storey erstellen
                        var createdStorey = await service.CreateStoreyAsync(storey);

                        // Erfolg zurückgeben
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
                    async ([FromServices] IStoreyService service, Guid id) =>
                    {
                        var storey = await service.GetStoreyByIdAsync(id);
                        if (storey != null)
                        {
                            return Results.Ok(storey);
                        }
                        else
                        {
                            return Results.NotFound(new { Message = "Storey not found."});
                        }
                    }
                )
                .WithName("GetStoreyById")
                .WithOpenApi()
                .WithTags("Storey");

            app.MapPut(
                    "/api/v3/assets/storeys/{id}",
                    async (
                        [FromServices] IStoreyService storeyservice,
                        [FromServices] IBuildingService buildingService,
                        Guid id,
                        Storey storey
                    ) =>
                    {
                        if (storey == null)
                            return Results.BadRequest("Storey data is required.");

                        // Überprüfung, ob das Gebäude existiert
                        var building = await buildingService.GetBuildingByIdAsync(
                            storey.building_id
                        );

                        if (building == null)
                            return Results.NotFound("Building not found.");

                        if (building.deleted_at != null) // Angenommen, das Gebäude hat ein 'DeletedAt' Feld
                            return Results.BadRequest("Building is deleted and cannot be modified.");

                        if (storey.deleted_at != null)
                            return Results.BadRequest("Storey is deleted and cannot be modified");

                        // Aktualisierung des Storeys
                        var updatedStorey = await storeyservice.UpdateStoreyAsync(storey, id);

                        if (updatedStorey == null)
                            return Results.NotFound("Storey not found or could not be updated.");

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
                        Guid id,
                        [FromQuery] bool permanent = false
                    ) =>
                    {
                        var deletedSucessfully = await service.DeleteStoreyAsync(id, permanent);

                        if (deletedSucessfully)
                            return Results.NoContent();

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
