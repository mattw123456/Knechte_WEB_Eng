using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace WebEngineering
{
    public class UnauthorizedMiddleware
    {
        private readonly RequestDelegate _next;

        public UnauthorizedMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            await _next(httpContext);

            if (
                !httpContext.Response.HasStarted
                && httpContext.Response.StatusCode == StatusCodes.Status401Unauthorized
            )
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                httpContext.Response.ContentType = "application/problem+json";
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Unauthorized",
                    Detail = "You are not authorized to access this resource.",
                    Instance = httpContext.Request.Path,
                };
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };
                var json = JsonSerializer.Serialize(problemDetails, options);
                await httpContext.Response.WriteAsync(json);
            }
        }
    }
}
