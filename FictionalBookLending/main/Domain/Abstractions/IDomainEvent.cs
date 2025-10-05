namespace FictionalBookLending.src.Domain.Abstractions
{
    public interface IDomainEvent
    {
        string EventType { get; }
        string Message { get; }
        IDictionary<string, string> Attributes { get; }
    }
}
