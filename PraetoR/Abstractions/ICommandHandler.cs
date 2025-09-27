namespace PraetoR
{
    /// <summary>
    /// Defines a handler for a message that returns a value.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to be handled.</typeparam>
    /// <typeparam name="TResponse">The type of response from the handler.</typeparam>
    public interface ICommandHandler<in TMessage, TResponse> where TMessage : ICommand<TResponse>
    {
        /// <summary>
        /// Handles a message and returns a response.
        /// </summary>
        /// <param name="message">The message object.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation, containing the handler's response.</returns>
        Task<TResponse> Handle(TMessage message, CancellationToken cancellationToken);
    }
    /// <summary>
    /// Defines a handler for a message that does not return a value.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to be handled.</typeparam>
    public interface ICommandHandler<in TMessage> where TMessage : ICommand
    {
        /// <summary>
        /// Handles a message.
        /// </summary>
        /// <param name="message">The message object.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the completion of the handling process.</returns>
        Task Handle(TMessage message, CancellationToken cancellationToken);
    }
}
