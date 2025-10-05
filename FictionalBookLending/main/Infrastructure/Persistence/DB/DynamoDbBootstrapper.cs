using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;

namespace FictionalBookLending.src.Infrastructure.Persistence.DB
{
    public class DynamoDbBootstrapper : IDynamoDbBootstrapper
    {
        private readonly IAmazonDynamoDB _dynamo;
        private readonly ILogger<DynamoDbBootstrapper> _logger;

        public DynamoDbBootstrapper(IAmazonDynamoDB dynamo, ILogger<DynamoDbBootstrapper> logger)
        {
            _dynamo = dynamo;
            _logger = logger;
        }

        public async Task InitializeAsync(CancellationToken ct)
        {

            _logger.LogInformation("Bootstrapping DynamoDB tables...");

            var existingTables = await _dynamo.ListTablesAsync(ct);

            // Ensure Books table exists
            if (!existingTables.TableNames.Contains("tbl_Books"))
            {
                _logger.LogInformation("Creating DynamoDB table: Books");

                var createBooks = new CreateTableRequest
                {
                    TableName = "tbl_Books",
                    BillingMode = BillingMode.PAY_PER_REQUEST,
                    AttributeDefinitions = new()
                {
                    new AttributeDefinition("id", ScalarAttributeType.S),

                },
                    KeySchema = new()
                {
                    new KeySchemaElement("id", KeyType.HASH),

                }
                };

                await _dynamo.CreateTableAsync(createBooks, ct);
                _logger.LogInformation("Books table created successfully");
            }

            // Ensure idempotency-records table exists
            if (!existingTables.TableNames.Contains("tbl_Idempotency-records"))
            {
                _logger.LogInformation("Creating DynamoDB table: tbl_Idempotency-records");

                var createIdem = new CreateTableRequest
                {
                    TableName = "tbl_Idempotency-records",
                    BillingMode = BillingMode.PAY_PER_REQUEST,
                    AttributeDefinitions = new()
                {
                    new AttributeDefinition("key", ScalarAttributeType.S)
                },
                    KeySchema = new()
                {
                    new KeySchemaElement("key", KeyType.HASH)
                }
                };

                await _dynamo.CreateTableAsync(createIdem, ct);

                // Optional: Enable TTL
                try
                {
                    await _dynamo.UpdateTimeToLiveAsync(new UpdateTimeToLiveRequest
                    {
                        TableName = "idempotency-records",
                        TimeToLiveSpecification = new TimeToLiveSpecification
                        {
                            AttributeName = "expiresAt",
                            Enabled = true
                        }
                    }, ct);
                    _logger.LogInformation("TTL enabled for idempotency-records table");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not enable TTL for idempotency-records");
                }

                _logger.LogInformation("Idempotency table created successfully");
            }

            _logger.LogInformation("DynamoDB bootstrap process completed successfully.");
        }
    }
}
