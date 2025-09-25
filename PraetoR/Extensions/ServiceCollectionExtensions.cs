using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace PraetoR
{
    /// <summary>
    /// Provides extension methods for setting up PraetoR services in an IServiceCollection.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Scans the specified assembly for PraetoR handlers and registers them in the IServiceCollection.
        /// It also registers the core PraetoR services.
        /// </summary>
        /// <param name="services">The IServiceCollection to add the services to.</param>
        /// <param name="assemblyToScan">The assembly to scan for handlers.</param>
        /// <returns>The same IServiceCollection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddPraetoR(this IServiceCollection services, Assembly assemblyToScan)
        {
            // 1. Register the main PraetoR dispatcher implementation.
            // A transient lifetime is appropriate as it holds no state.
            services.AddTransient<IPraetoR, PraetoR>();

            // 2. Scan and register all IMessageHandler<TMessage, TResponse> implementations (with return value).
            RegisterHandlers(services, typeof(IOperationHandler<,>), assemblyToScan);

            // 3. Scan and register all IMessageHandler<TMessage> implementations (without return value).
            RegisterHandlers(services, typeof(IOperationHandler<>), assemblyToScan);

            // 4. Scan and register all IEventHandler<TEvent> implementations.
            RegisterHandlers(services, typeof(IDictumHandler<>), assemblyToScan);

            return services;

        }

        private static void RegisterHandlers(IServiceCollection services, Type handlerInterface, Assembly assembly)
        {
            assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface))
                .ToList()
                .ForEach(handlerType =>
                {
                    // For each found handler, register it against its implemented interface.
                    var serviceType = handlerType.GetInterfaces()
                        .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface);
                    services.AddTransient(serviceType, handlerType);
                });
        }

    }
}
