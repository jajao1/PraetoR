
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

        public Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
        {
            var commandType = command.GetType();

            // 1. Resolve o handler (lógica inalterada)
            var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResponse));
            var handler = _serviceProvider.GetService(handlerType)
                ?? throw new InvalidOperationException($"No handler found for command type {commandType.Name}");

            // 2. Define a execução do handler como o passo final
            RequestHandlerDelegate<TResponse> handlerDelegate = () =>
            {
                var handleMethod = handler.GetType().GetMethod("Handle", new[] { commandType, typeof(CancellationToken) });
                return (Task<TResponse>)handleMethod.Invoke(handler, [command, cancellationToken]);
            };

            // 3. Constrói o tipo do behavior e o tipo da coleção que o contém
            var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(commandType, typeof(TResponse));
            var enumerableBehaviorType = typeof(IEnumerable<>).MakeGenericType(behaviorType);

            // 4. Resolve a coleção de behaviors de forma segura
            //    Se GetService retornar null (nenhum registrado), usamos uma coleção vazia.
            var behaviors = (_serviceProvider.GetService(enumerableBehaviorType) as IEnumerable<object> ?? Enumerable.Empty<object>())
                .Reverse();

            // 5. Constrói e executa o pipeline (lógica inalterada)
            var pipeline = behaviors.Aggregate(
                handlerDelegate,
                (next, behavior) => () => ((IPipelineBehavior<ICommand<TResponse>, TResponse>)behavior).Handle(command, next, cancellationToken)
            );

            return pipeline();
        }
        public Task Send(ICommand command, CancellationToken cancellationToken = default)
        {
            var commandType = command.GetType();

            // Constrói o tipo do handler (ICommandHandler<TRequest>)
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(commandType);
            var handler = _serviceProvider.GetService(handlerType)
                ?? throw new InvalidOperationException($"No handler found for command type {commandType.Name}");

            // O delegado para o handler principal
            CommandHandlerDelegate handlerDelegate = () =>
            {
                var handleMethod = handler.GetType().GetMethod("Handle", new[] { commandType, typeof(CancellationToken) });
                return (Task)handleMethod.Invoke(handler, new object[] { command, cancellationToken });
            };

            // Constrói o tipo do behavior para comandos SEM retorno
            var behaviorType = typeof(IPipelineBehavior<>).MakeGenericType(commandType);
            var enumerableBehaviorType = typeof(IEnumerable<>).MakeGenericType(behaviorType);

            // Resolve os behaviors de forma segura (retornando vazio se não houver nenhum)
            var behaviors = (_serviceProvider.GetService(enumerableBehaviorType) as IEnumerable<object> ?? Enumerable.Empty<object>())
                .Reverse();

            // Constrói a cadeia de pipeline
            var pipeline = behaviors.Aggregate(
                handlerDelegate,
                (next, behavior) => () => ((IPipelineBehavior<ICommand>)behavior).Handle(command, next, cancellationToken)
            );

            return pipeline();
        }
    }
}
