namespace PraetoR
{
    /// <summary>
    /// Defines the contract for the central dispatcher, 
    /// responsible for routing messages to their respective handlers
    /// and publishing events to all registered handlers.
    /// </summary>
    public interface IPraetoR
    {
        /// <summary>
        /// Sends a message to a single handler and awaits a response.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response from the handler.</typeparam>
        /// <param name="command">The message object that implements IMessage<TResponse>.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, 
        /// containing the handler's response.
        /// </returns>
        Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
        /// <summary>
        /// Publishes an event to all registered event handlers.
        /// This is a "fire-and-forget" operation that does not return a value but can be awaited.
        /// </summary>
        /// <param name="event">The event object that implements IEvent.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the completion of the publication to all handlers.
        /// </returns>
        Task Publish(IDictum @event, CancellationToken cancellationToken = default);
        /// <summary>
        /// Sends a message to a single handler and awaits a response.
        /// </summary>
        /// <param name="command">The message object that implements IMessage<TResponse>.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, 
        /// containing the handler's response.
        /// </returns>
        Task Send(ICommand command, CancellationToken cancellationToken = default);

    }
}
