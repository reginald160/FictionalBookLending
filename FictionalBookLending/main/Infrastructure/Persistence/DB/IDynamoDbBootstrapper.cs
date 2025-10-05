
namespace FictionalBookLending.src.Infrastructure.Persistence.DB
{
    public interface IDynamoDbBootstrapper
    {
        Task InitializeAsync(CancellationToken ct);
    }
}
