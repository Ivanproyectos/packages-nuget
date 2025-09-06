using FluentValidation;
using FluentValidationInterceptor.Middlewares;

namespace FluentValidationInterceptor
{
    public static class DependencyContainer
    {
        [Obsolete("Use AddRequestValidation instead. This method will be removed in v2.0.0")]
        public static IServiceCollection AddFluentValidationInterceptor(
            this IServiceCollection services
        )
        {
            return services.AddRequestValidation();
        }

        [Obsolete("Use UseRequestValidation instead. This method will be removed in v2.0.0")]
        public static IApplicationBuilder UseFluentValidationInterceptor(
            this IApplicationBuilder builder
        )
        {
            return builder.UseRequestValidation();
        }

        public static IServiceCollection AddRequestValidation(this IServiceCollection services)
        {
            var assemblies = AppDomain
                .CurrentDomain.GetAssemblies()
                .Where(assembly =>
                    !assembly.IsDynamic
                    && Path.GetDirectoryName(assembly.Location)?.Contains("bin") == true
                    && !assembly.Location.Contains("System")
                    && !assembly.Location.Contains("Microsoft")
                    && !assembly.Location.Contains("NuGet")
                    && !assembly.ManifestModule.Name.Equals(
                        "FluentValidation.dll",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .ToList();

            foreach (var assembly in assemblies)
            {
                try
                {
                    services.AddValidatorsFromAssembly(assembly);
                }
                catch
                {
                    // Ignora assemblies que no tienen validadores
                }
            }
            return services;
        }

        public static IApplicationBuilder UseRequestValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ValidationMiddleware>();
        }
    }
}
