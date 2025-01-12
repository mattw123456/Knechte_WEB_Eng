using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace WebEngineering
{
    public static class AuthentifizierungsErweiterung
    {
        public static async Task AddJwtAuthenticationAsync(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment
        )
        {
            var jwtIssuer = GetJwtIssuer(configuration, environment);
            var jwtAuthority = GetJwtAuthority(configuration, environment);
            var keys = await FetchJsonWebKeysAsync(configuration, environment);

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKeys = keys.Keys,
                        ValidateIssuer = false,
                        ValidIssuer = jwtIssuer,
                        ValidateAudience = true,
                        ValidAudience = configuration["KEYCLOAK_AUDIENCE"],
                        ValidateLifetime = true,
                    };
                });
        }

        private static string GetJwtIssuer(
            IConfiguration configuration,
            IHostEnvironment environment
        )
        {
            var baseUri = $"http://{configuration["KEYCLOAK_HOST"]}";
            var port = environment.IsDevelopment()
                ? $":{configuration["KEYCLOAK_PORT"]}"
                : string.Empty;
            return $"{baseUri}{port}/auth/realms/{configuration["KEYCLOAK_REALM"]}";
        }

        private static string GetJwtAuthority(
            IConfiguration configuration,
            IHostEnvironment environment
        )
        {
            return GetJwtIssuer(configuration, environment);
        }

        private static async Task<JsonWebKeySet> FetchJsonWebKeysAsync(
            IConfiguration configuration,
            IHostEnvironment environment
        )
        {
            string json = string.Empty;
            int maxRetries = 3;
            int delayBetweenRetries = 5000;
            var url = environment.IsDevelopment()
                ? $"http://{configuration["KEYCLOAK_HOST"]}:{configuration["KEYCLOAK_PORT"]}/auth/realms/{configuration["KEYCLOAK_REALM"]}/protocol/openid-connect/certs"
                : $"http://{configuration["KEYCLOAK_HOST"]}/auth/realms/{configuration["KEYCLOAK_REALM"]}/protocol/openid-connect/certs";

            using var httpClient = new HttpClient();
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    json = await httpClient.GetStringAsync(url);
                    break;
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Attempt {attempt + 1} failed: {ex.Message}");
                    if (attempt < maxRetries - 1)
                    {
                        await Task.Delay(delayBetweenRetries);
                    }
                }
            }

            if (string.IsNullOrEmpty(json))
            {
                Console.WriteLine("No Key fetched!");
                json = "{\"keys\":[]}"; // Use an empty key if Keycloak was not available
            }

            return new JsonWebKeySet(json);
        }
    }
}
