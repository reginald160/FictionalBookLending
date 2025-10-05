using Amazon.SimpleSystemsManagement.Model;
using Amazon.SimpleSystemsManagement;

namespace FictionalBookLending.src.Infrastructure.Configuration
{
    public sealed record SqsConfiguration
    {
        public int Id { get; set; }
        public string MainQueueUrl { get; set; } = string.Empty;
        public string DeadLetterQueueUrl { get; set; } = string.Empty;
        public string FifoQueueUrl { get; set; } = string.Empty;



        public static async Task<SqsConfiguration> GetSqsConfigurationAsync(IConfiguration configuration)
        {
            try
            {
                // Priority 1: Environment Variables
                var mainQueueUrl = Environment.GetEnvironmentVariable("sqs_main_queue_url") ?? configuration["sqs_main_queue_url:value"];
                var dlqUrl = Environment.GetEnvironmentVariable("sqs_dead_letter_queue_url") ?? configuration["sqs_dead_letter_queue_url:value"];
                var fifoQueueUrl = Environment.GetEnvironmentVariable("sqs_fifo_queue_url") ?? configuration["sqs_fifo_queue_url:value"];

                if (!string.IsNullOrEmpty(mainQueueUrl))
                {
                    Console.WriteLine("Using SQS URLs from Environment Variables");
                    return new SqsConfiguration
                    {
                        MainQueueUrl = mainQueueUrl,
                        DeadLetterQueueUrl = dlqUrl ?? "",
                        FifoQueueUrl = fifoQueueUrl ?? ""
                    };
                }

                // Priority 2: appsettings.json
                mainQueueUrl = configuration["Sqs:MainQueueUrl"];
                if (!string.IsNullOrEmpty(mainQueueUrl))
                {
                    Console.WriteLine("Using SQS URLs from appsettings.json");
                    return new SqsConfiguration
                    {
                        MainQueueUrl = mainQueueUrl,
                        DeadLetterQueueUrl = configuration["Sqs:DeadLetterQueueUrl"] ?? "",
                        FifoQueueUrl = configuration["Sqs:FifoQueueUrl"] ?? ""
                    };
                }

                // Priority 3: AWS SSM Parameter Store
                Console.WriteLine("Attempting to get SQS URLs from AWS SSM Parameter Store");
                var env = configuration["Environment"] ?? "dev";

                using var ssmClient = new AmazonSimpleSystemsManagementClient();

                var mainQueueTask = ssmClient.GetParameterAsync(new GetParameterRequest
                {
                    Name = $"/{env}/sqs/main-queue-url"
                });

                var dlqTask = ssmClient.GetParameterAsync(new GetParameterRequest
                {
                    Name = $"/{env}/sqs/dlq-url"
                });

                var fifoQueueTask = ssmClient.GetParameterAsync(new GetParameterRequest
                {
                    Name = $"/{env}/sqs/fifo-queue-url"
                });

                await Task.WhenAll(mainQueueTask, dlqTask, fifoQueueTask);

                Console.WriteLine("Successfully retrieved SQS URLs from AWS SSM");
                return new SqsConfiguration
                {
                    MainQueueUrl = mainQueueTask.Result.Parameter.Value,
                    DeadLetterQueueUrl = dlqTask.Result.Parameter.Value,
                    FifoQueueUrl = fifoQueueTask.Result.Parameter.Value
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting SQS configuration: {ex.Message}");
                throw new Exception("SQS configuration not found in any configuration source", ex);
            }
        }

    }

}
