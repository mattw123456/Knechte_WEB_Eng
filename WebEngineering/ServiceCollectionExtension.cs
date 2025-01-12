using WebEngineering.Buildings;
using WebEngineering.Room;
using WebEngineering.Status;
using WebEngineering.Storey;

namespace WebEngineering
{
    public static class ServiceCollectionExtension
    {
        public static void AddApplicationServices(this IServiceCollection services) {

            services.AddScoped<IStatusService, StatusService>();
            services.AddScoped<IBuildingService, BuildingService>();
            services.AddScoped<IStoreyService, StoreyService>();
            services.AddScoped<IRoomService, RoomService>();
        }
    }
}
