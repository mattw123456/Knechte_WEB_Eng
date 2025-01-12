using Microsoft.EntityFrameworkCore;

namespace WebEngineering
{
    public static class DatenbankExtension
    {
        public static void AddPostgresDbContext(this IServiceCollection services, IConfiguration configuration) {

            var dbConnectionString =
           $"Server={configuration["POSTGRES_ASSETS_HOST"]};Port={configuration["POSTGRES_ASSETS_PORT"]};Database={configuration["POSTGRES_ASSETS_DBNAME"]};Username={configuration["POSTGRES_ASSETS_USER"]};Password={configuration["POSTGRES_ASSETS_PASSWORD"]}";

            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(dbConnectionString));
        }
    }
}
