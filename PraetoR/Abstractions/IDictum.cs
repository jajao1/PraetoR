namespace PraetoR
{
    /// <summary>
    /// Represents an event that has occurred in the system.
    /// Events are published via the IPraetoR and handled by one or more IEventHandler instances.
    /// This is a marker interface; it does not contain any members.
    /// </summary>
    public interface IDictum
    {
        // This interface is intentionally left empty.
        // It serves as a constraint for event classes that can be published.
    }
}
