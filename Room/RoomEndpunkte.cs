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
                        [FromServices] ILogger<IRoomService> logger,
                        [FromQuery] Guid? storey_id = null,
                        [FromQuery] bool include_deleted = false
                    ) =>
                    {
                        logger.LogInformation(
                            "Fetching all rooms (storey_id={storey_id}, include_deleted={include_deleted})",
                            storey_id,
                            include_deleted
                        );

                        var rooms = await service.GetAllRoomsAsync(storey_id, include_deleted);
                        return new { rooms = rooms };
                    }
                )
                .WithName("GetAllRooms")
                .WithOpenApi()
                .WithTags("Room");

            app.MapPost(
                    "/api/v3/assets/rooms",
                    async (
                        [FromServices] IStoreyService storeyService,
                        [FromServices] IRoomService roomService,
                        [FromServices] ILogger<IRoomService> logger,
                        [FromBody] Room room
                    ) =>
                    {
                        logger.LogInformation("Creating a new room with data: {@room}", room);

                        var storey = await storeyService.GetStoreyByIdAsync(room.storey_id);

                        if (storey == null || storey.deleted_at != null)
                        {
                            logger.LogWarning(
                                "Failed to create room: storey_id={storey_id} not found or deleted",
                                room.storey_id
                            );
                            return Results.BadRequest(
                                new { Message = "Storey not found or deleted." }
                            );
                        }

                        room.storey_id = room.storey_id;

                        var createdRoom = await roomService.CreateRoomAsync(room);

                        logger.LogInformation(
                            "Room successfully created with ID={createdRoomId}",
                            createdRoom.id
                        );

                        return Results.Created(
                            $"/api/v3/assets/rooms/{createdRoom.id}",
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
                    async (
                        [FromServices] IRoomService service,
                        [FromServices] ILogger<IRoomService> logger,
                        Guid id
                    ) =>
                    {
                        logger.LogInformation("Fetching room details for ID={id}", id);

                        var room = await service.GetRoomByIdAsync(id);

                        if (room == null)
                        {
                            logger.LogWarning("Room with ID={id} not found", id);
                            return Results.NotFound(new { Message = "Room not found." });
                        }

                        logger.LogInformation("Room details fetched for ID={id}", id);
                        return Results.Ok(room);
                    }
                )
                .WithName("GetRoomById")
                .WithOpenApi()
                .WithTags("Room");

            app.MapPut(
                    "/api/v3/assets/rooms/{id}",
                    async (
                        [FromServices] IStoreyService storeyService,
                        [FromServices] IRoomService roomService,
                        [FromServices] ILogger<IRoomService> logger,
                        Guid id,
                        Room room
                    ) =>
                    {
                        logger.LogInformation("Updating room with ID={id} and data: {@room}", id, room);

                        if (room == null)
                            return Results.BadRequest("Room data is required.");

                        var storey = await storeyService.GetStoreyByIdAsync(room.storey_id);

                        if (storey == null || storey.deleted_at != null)
                        {
                            logger.LogWarning(
                                "Failed to update room: storey_id={storey_id} not found or deleted",
                                room.storey_id
                            );
                            return Results.BadRequest("Storey not found or deleted.");
                        }

                        if (room.deleted_at != null)
                        {
                            logger.LogWarning("Failed to update room: Room ID={id} is deleted", id);
                            return Results.BadRequest("Room is deleted and cannot be modified");
                        }

                        var updatedRoom = await roomService.UpdateRoomAsync(room, id);

                        if (updatedRoom == null)
                        {
                            logger.LogWarning("Room with ID={id} not found for update", id);
                            return Results.NotFound("Room not found or could not be updated.");
                        }

                        logger.LogInformation("Room with ID={id} successfully updated", id);
                        return Results.Ok(updatedRoom);
                    }
                )
                .WithName("UpdateRoom")
                .WithOpenApi()
                .WithTags("Room")
                .RequireAuthorization();

            app.MapDelete(
                    "/api/v3/assets/rooms/{id}",
                    async (
                        [FromServices] IRoomService service,
                        [FromServices] ILogger<IRoomService> logger,
                        Guid id,
                        [FromQuery] bool permanent = false
                    ) =>
                    {
                        logger.LogInformation("Deleting room with ID={id} (permanent={permanent})", id, permanent);

                        var deletedSuccessfully = await service.DeleteRoomAsync(id, permanent);

                        if (deletedSuccessfully)
                        {
                            logger.LogInformation("Room with ID={id} successfully deleted", id);
                            return Results.NoContent();
                        }

                        logger.LogWarning("Failed to delete room with ID={id}", id);
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
