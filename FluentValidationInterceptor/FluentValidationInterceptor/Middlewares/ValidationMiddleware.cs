using System.Text;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using FluentValidationInterceptor.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace FluentValidationInterceptor.Middlewares
{
    public class ValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ValidationMiddleware> _logger;

        public ValidationMiddleware(
            RequestDelegate next,
            IServiceProvider serviceProvider,
            ILogger<ValidationMiddleware> logger
        )
        {
            _next = next;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!ShouldValidateRequest(context.Request.Method))
            {
                await _next(context);
                return;
            }

            context.Request.EnableBuffering();

            var endpoint = context.GetEndpoint();
            var requestType = GetValidationTargetType(endpoint);

            if (requestType == null)
            {
                await _next(context);
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            await ValidateRequest(context, requestType, scope.ServiceProvider);
        }

        private async Task ValidateRequest(
            HttpContext context,
            Type requestType,
            IServiceProvider scopedServiceProvider
        )
        {
            var originalBodyPosition = context.Request.Body.Position;

            try
            {
                var body = await ReadRequestBodyAsync(context.Request);

                if (string.IsNullOrEmpty(body))
                {
                    context.Request.Body.Position = originalBodyPosition;
                    await _next(context);
                    return;
                }

                var model = JsonSerializer.Deserialize(
                    body,
                    requestType,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (model == null)
                {
                    context.Request.Body.Position = originalBodyPosition;
                    await _next(context);
                    return;
                }

                var validator = GetValidatorForType(requestType, scopedServiceProvider);
                if (validator == null)
                {
                    _logger.LogInformation(
                        "No validator found for type {RequestType}",
                        requestType.Name
                    );
                    context.Request.Body.Position = originalBodyPosition;
                    await _next(context);
                    return;
                }

                var validationResult = await validator.ValidateAsync(
                    new ValidationContext<object>(model)
                );
                if (!validationResult.IsValid)
                {
                    await HandleValidationFailure(context, validationResult);
                    return;
                }

                context.Request.Body.Position = originalBodyPosition;
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el middleware de validación");
                context.Request.Body.Position = originalBodyPosition;
                await _next(context);
            }
        }

        private bool ShouldValidateRequest(string method)
        {
            return method.Equals("POST", StringComparison.OrdinalIgnoreCase)
                || method.Equals("PUT", StringComparison.OrdinalIgnoreCase)
                || method.Equals("PATCH", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            var currentPosition = request.Body.Position;

            request.Body.Position = 0;
            using var streamReader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await streamReader.ReadToEndAsync();

            request.Body.Position = currentPosition;
            return body;
        }

        private IValidator? GetValidatorForType(Type requestType, IServiceProvider serviceScope)
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(requestType);
            return serviceScope.GetService(validatorType) as IValidator;
        }

        private Type? GetValidationTargetType(Endpoint? endpoint)
        {
            if (endpoint == null)
                return null;

            var controllerActionDescriptor =
                endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
            if (controllerActionDescriptor == null)
                return null;

            var parameters = controllerActionDescriptor.MethodInfo.GetParameters();
            var bodyParameter = parameters.FirstOrDefault(p =>
                p.GetCustomAttributes(typeof(FromBodyAttribute), false).Any()
            );

            return bodyParameter?.ParameterType;
        }

        private async Task HandleValidationFailure(
            HttpContext context,
            ValidationResult validationResult
        )
        {
            var failures = validationResult
                .Errors.GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            var response = new ProblemResultDto
            {
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Detail = "See the errors property for details.",
                Instance = context.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Errors = failures,
            };
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";
            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
