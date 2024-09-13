using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using LoanGeneratorAPI.Data;
using Microsoft.Extensions.Logging;

namespace LoanGeneratorAPI.Middleware
{

    public class ValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ValidationMiddleware> _logger;

        public ValidationMiddleware(RequestDelegate next, ILogger<ValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<RequiresValidationAttribute>() != null)
            {
                context.Request.EnableBuffering();

                // Deserialize the request body to check for validation
                var requestBody = await JsonSerializer.DeserializeAsync<LoanRequest>(
                    context.Request.Body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                context.Request.Body.Position = 0;

                // Validate the model
                var validationContext = new ValidationContext(requestBody);
                var validationResults = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(
                    requestBody, validationContext, validationResults, true);

                if (!isValid)
                {
                    // Log validation failure
                    _logger.LogWarning("Validation failed for request at {Time}: {ValidationErrors}", DateTime.UtcNow, validationResults);

                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(validationResults);
                    return;
                }
                // Log validation success
                _logger.LogInformation("Validation successful for request at {Time}", DateTime.UtcNow);

            }

            await _next(context);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class RequiresValidationAttribute : Attribute
    {
    }
}

