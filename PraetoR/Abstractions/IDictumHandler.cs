namespace PraetoR
{
    /// <summary>
    /// Defines a handler for a specific event type.
    /// An event can have multiple event handlers.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to be handled.</typeparam>
    public interface IDictumHandler<in TEvent> where TEvent : IDictum
    {
        /// <summary>
        /// Handles the event. This method is called by the PraetoR when an event is published.
        /// </summary>
        /// <param name="event">The event object.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the completion of the handling process.</returns>
        Task Handle(TEvent @event, CancellationToken cancellationToken);
    }
}
