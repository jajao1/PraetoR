
using PraetoR;
using Microsoft.Extensions.DependencyInjection;

namespace PraetoR
{
    /// <summary>
    /// The concrete implementation of IPraetoR. It uses an IServiceProvider
    /// to resolve and dispatch messages and events to their respective handlers.
    /// </summary>
    public class PraetoR : IPraetoR
    {
        private readonly IServiceProvider _serviceProvider;

        public PraetoR(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Publish(IDictum @event, CancellationToken cancellationToken = default)
        {
            var eventType = @event.GetType();
            var handlerType = typeof(IDictumHandler<>).MakeGenericType(eventType);

            var handlers = _serviceProvider.GetServices(handlerType);

            if (!handlers.Any()) 
            {
                return;
            }

            var handleMethod = handlerType.GetMethod("Handle", [eventType, typeof(CancellationToken)]);

            if(handleMethod == null)
            {
                throw new InvalidOperationException($"Method 'Handle' not found on handler for event '{eventType.Name}'");
            }


            var tasks = handlers.Select(handler =>
                    (Task)handleMethod.Invoke(handler, [@event, cancellationToken])
                );

            await Task.WhenAll(tasks);
        }

        public Task<TResponse> Send<TResponse>(IOperation<TResponse> message, CancellationToken cancellationToken = default)
        {
            var messageType = message.GetType();
            var handlerType = typeof (IOperationHandler<,>).MakeGenericType(messageType, typeof(TResponse));
            
            var handler = _serviceProvider.GetService(handlerType);

            if(handler == null)
            {
                throw new InvalidOperationException($"No handler found for message type {messageType.Name}");
            }

            return (Task<TResponse>)handler.GetType()
                .GetMethod("Handle")
                .Invoke(handler, [message, cancellationToken]);
        }

        public Task Send(IOperation message, CancellationToken cancellationToken = default)
        {
            var messageType = message.GetType();
            var handlerType = typeof(IOperationHandler<>).MakeGenericType(messageType);

            var handler = _serviceProvider.GetService(handlerType);
            if(handler == null)
            {
                throw new InvalidOperationException($"No handler found for message type {messageType.Name}");
            }

            return (Task)handler.GetType()
                .GetMethod("Handle")
                .Invoke(handler, [message, cancellationToken]);
        }
    }
}
