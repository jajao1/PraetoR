namespace PraetoR
{
    // Delegado para o pipeline de comandos com retorno
    public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

    // Delegado para o pipeline de comandos SEM retorno
    public delegate Task CommandHandlerDelegate();

    // Interface para behaviors de comandos COM retorno
    public interface IPipelineBehavior<in TRequest, TResponse> where TRequest : ICommand<TResponse>
    {
        Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
    }

    // NOVA INTERFACE: Para behaviors de comandos SEM retorno
    public interface IPipelineBehavior<in TRequest> where TRequest : ICommand
    {
        Task Handle(TRequest request, CommandHandlerDelegate next, CancellationToken cancellationToken);
    }
}
