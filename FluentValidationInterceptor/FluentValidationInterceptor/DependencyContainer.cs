using FluentValidation;
using FluentValidationInterceptor.Middlewares;

namespace FluentValidationInterceptor
{
    public static class DependencyContainer
    {
        public static IServiceCollection AddFluentValidationInterceptor(
            this IServiceCollection services
        )
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

        public static IApplicationBuilder UseFluentValidationInterceptor(
            this IApplicationBuilder builder
        )
        {
            return builder.UseMiddleware<ValidationMiddleware>();
        }
    }
}
