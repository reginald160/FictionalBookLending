using FictionalBookLending.src.Domain.Abstractions;

namespace FictionalBookLending.src.Domain.Common
{
    public class Entity
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected void Raise(IDomainEvent @event)
        {
            _domainEvents.Add(@event);
        }

        public void ClearEvents() => _domainEvents.Clear();
    }
}
