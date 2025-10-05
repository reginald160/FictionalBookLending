using System.Diagnostics;

namespace FictionalBookLending.src.Application.Contracts
{
    public abstract record BaseActivityTrace
    {
        public string TraceId  { get; init; } = Activity.Current?.Id ?? Guid.NewGuid().ToString();

    }
}
