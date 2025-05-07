using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
            
            // Cas de 400 avec erreurs de validation (via ProblemDetails ou ModelState)
            if (context.Response.StatusCode == (int)HttpStatusCode.BadRequest && context.Items.ContainsKey("ValidationErrors"))
            {
                var errors = context.Items["ValidationErrors"];
                await WriteJsonResponse(context, HttpStatusCode.BadRequest, new
                {
                    error = "Validation failed",
                    details = errors
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Une erreur inattendue est survenue.");

            await WriteJsonResponse(context, HttpStatusCode.InternalServerError, new
            {
                error = "Une erreur interne est survenue",
                details = ex.Message
            });
        }
    }

    private async Task WriteJsonResponse(HttpContext context, HttpStatusCode statusCode, object data)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(data, options));
    }
}
