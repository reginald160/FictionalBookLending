using Amazon.DynamoDBv2.DataModel;

namespace FictionalBookLending.src.Domain.Entities
{
    [DynamoDBTable("tbl_Idempotency-records")]
    public class IdempotencyRecord
    {
        [DynamoDBHashKey("key")]
        public string Key { get; set; } = default!;

        [DynamoDBProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DynamoDBProperty("response")]
        public string? Response { get; set; }
    }
}
