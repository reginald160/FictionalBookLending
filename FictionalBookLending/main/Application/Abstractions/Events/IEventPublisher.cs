using FictionalBookLending.src.Domain.Abstractions;

namespace FictionalBookLending.src.Application.Abstractions.Events
{
    public interface IEventPublisher
    {
        Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken ct);
    }
}
