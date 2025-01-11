using WebEngineering.Buildings;
using WebEngineering.Room;
using WebEngineering.Status;
using WebEngineering.Storey;

namespace WebEngineering
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddEnvironmentVariables();

            var myAllowSpecificOrigins = "_myAllowSpecificOrigins";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    name: myAllowSpecificOrigins,
                    policy =>
                    {
                        policy
                            .AllowAnyOrigin() // Allow all origins
                            .AllowAnyHeader() // Allow all headers
                            .AllowAnyMethod(); // Allow all methods
                    }
                );
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddApplicationServices();
            builder.Services.AddPostgresDbContext(builder.Configuration);

            // Add authentication services
            await builder.Services.AddJwtAuthenticationAsync(
                builder.Configuration,
                builder.Environment
            );

            // Add authorization services
            builder.Services.AddAuthorization();

            var app = builder.Build();

            

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<UnauthorizedMiddleware>();
            app.UseAuthorization();

            app.UseCors(myAllowSpecificOrigins);
            app.UseHttpsRedirection();

            app.AddStatusEndpunkte();
            app.AddBuildingEndpunkte();
            app.AddStoreyEndpunkte();
            app.AddRoomEndpunkte();

            app.Run();
        }
    }
}
