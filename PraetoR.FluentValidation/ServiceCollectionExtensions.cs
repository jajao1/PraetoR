using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
namespace PraetoR.FluentValidation
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPraetorFluentValidation(this IServiceCollection services, Assembly assembly)
        {

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<>), typeof(ValidationBehavior<,>));
            services.AddValidatorsFromAssembly(assembly);

            return services;
        }
    }
}
