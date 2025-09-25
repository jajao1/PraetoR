namespace PraetoR
{
    /// <summary>
    /// Represents a message (typically a Command) that does not return a value.
    /// It is handled by a single IMessageHandler.
    /// </summary>
    public interface IOperation
    {
        // This is a marker interface and is intentionally left empty.
    }
    /// <summary>
    /// Represents a message that expects a single response from a single handler.
    /// This serves as the base interface for both Commands (actions that change state) 
    /// and Queries (requests for data) in a CQRS-style architecture.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    public interface IOperation<out TResponse>
    {
        // This is a marker interface and is intentionally left empty.
        // It's used to define a request and its expected response type.
    }
}
