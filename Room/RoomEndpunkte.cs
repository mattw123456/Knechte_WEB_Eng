using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebEngineering.Buildings;
using WebEngineering.Storey;

namespace WebEngineering.Room
{
    public static class RoomEndpunkte
    {
        public static void AddRoomEndpunkte(this IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "/api/v3/assets/rooms",
                    async (
                        [FromServices] IRoomService service,
                        [FromQuery] Guid? storey_id = null,
                        [FromQuery] bool include_deleted = false
                    ) =>
                    {
                        var room = await service.GetAllRoomsAsync(storey_id, include_deleted);
                        return new { rooms = room };
                    }
                )
                .WithName("GetAllRooms")
                .WithOpenApi()
                .WithTags("Room");

            app.MapPost(
                    "/api/v3/assets/rooms",
                    async (
                        [FromServices] IStoreyService service,
                        [FromServices] IRoomService roomservice,
                        [FromBody] Room room
                    ) =>
                    {
                        // Überprüfen, ob das Storey existiert und nicht gelöscht ist
                        var storey = await service.GetStoreyByIdAsync(room.storey_id);

                        if (storey == null || storey.deleted_at != null)
                        {
                            // Fehler, wenn das Storey nicht existiert oder gelöscht ist
                            return Results.BadRequest(
                                new { Message = "Storey not found or deleted." }
                            );
                        }

                        // Storey mit der Gebäude-ID verknüpfen
                        room.storey_id = room.storey_id;

                        // Storey erstellen
                        var createdRoom = await roomservice.CreateRoomAsync(room);

                        // Erfolg zurückgeben
                        return Results.Created(
                            $"/api/v3/assets/storeys/{createdRoom.id}",
                            createdRoom
                        );
                    }
                )
                .WithName("CreateRoom")
                .WithOpenApi()
                .WithTags("Room")
                .RequireAuthorization();

            app.MapGet(
                    "/api/v3/assets/rooms/{id}",
                    async ([FromServices] IRoomService service, Guid id) =>
                    {
                        var room = await service.GetRoomByIdAsync(id);
                        return room is not null
                            ? Results.Ok(room)
                            : Results.NotFound(new { Message = "Room not found." });
                    }
                )
                .WithName("GetRoomById")
                .WithOpenApi()
                .WithTags("Room");

            app.MapPut(
                    "/api/v3/assets/rooms/{id}",
                    async (
                        [FromServices] IStoreyService storeyservice,
                        [FromServices] IRoomService roomservice,
                        Guid id,
                        Room room
                    ) =>
                    {
                        if (room == null)
                            return Results.BadRequest("Room data is required.");

                        // Überprüfung, ob das Storey existiert
                        var storey = await storeyservice.GetStoreyByIdAsync(room.storey_id);

                        if (storey == null)
                            return Results.NotFound("Storey not found.");

                        if (storey.deleted_at != null) // Angenommen, das Storey hat ein 'DeletedAt' Feld
                            return Results.BadRequest("Storey is deleted and cannot be modified.");

                        if (room.deleted_at != null)
                            return Results.BadRequest("Room is deleted and cannot be modified");

                        // Aktualisierung des Storeys
                        var updatedRoom = await roomservice.UpdateRoomAsync(room, id);

                        if (updatedRoom == null)
                            return Results.NotFound("Room not found or could not be updated.");

                        return Results.Ok(updatedRoom);
                    }
                )
                .WithName("UpdatRoom")
                .WithOpenApi()
                .WithTags("Room")
                .RequireAuthorization();

            app.MapDelete(
                    "/api/v3/assets/rooms/{id}",
                    async (
                        [FromServices] IRoomService service,
                        Guid id,
                        [FromQuery] bool permanent = false
                    ) =>
                    {
                        var deletedSucessfully = await service.DeleteRoomAsync(id, permanent);

                        if (deletedSucessfully)
                            return Results.NoContent();

                        return Results.BadRequest();
                    }
                )
                .WithName("DeleteRoom")
                .WithOpenApi()
                .WithTags("Room")
                .RequireAuthorization();
        }
    }
}
