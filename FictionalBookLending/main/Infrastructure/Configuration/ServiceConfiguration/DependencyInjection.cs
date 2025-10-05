
using Amazon.DynamoDBv2;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Amazon.SQS;
using FictionalBookLending.src.Application.Abstractions;
using FictionalBookLending.src.Application.Abstractions.Events;
using FictionalBookLending.src.Infrastructure.Configuration;
using FictionalBookLending.src.Infrastructure.Messages.AWS;
using FictionalBookLending.src.Infrastructure.Persistence.Cache;
using FictionalBookLending.src.Infrastructure.Persistence.DB;
using FictionalBookLending.src.Infrastructure.Persistence.Repository;
using StackExchange.Redis;
using Polly;
using FictionalBookLending.main.Application.Abstractions;

namespace FictionalBookLending.src.Infrastructure.Configuration.ServiceConfiguration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg)
        {
            services.AddDefaultAWSOptions(cfg.GetAWSOptions());
            services.AddAWSService<IAmazonDynamoDB>();
            var mainQueueUrl = Environment.GetEnvironmentVariable("sqs_main_queue_url") ?? cfg["sqs_main_queue_url:value"];

            services.AddSingleton<IAmazonSQS>(sp =>
            {
                var config = new AmazonSQSConfig
                {
                    ServiceURL = mainQueueUrl
                };
                return new AmazonSQSClient(config);
            });
            services.AddScoped<IDynamoDbBootstrapper, DynamoDbBootstrapper>();
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IIdempotencyService, IdempotencyRepository>();
            services.AddScoped<IEventPublisher, SQSEventPublishert>();
            var redisConnectionString = Environment.GetEnvironmentVariable("redis_connection_string") ??
                cfg.GetValue<string>("redis_connection_string:value")
                     ?? throw new ArgumentNullException("redis connection string is null");

            ConfigureRedis(services);

            services.AddSingleton<IConnectionMultiplexer>(
                _ => ConnectionMultiplexer.Connect(redisConnectionString));

            services.AddSingleton<ICacheService, RedisCacheService>();

            return services;

        }

        public static void ConfigureRedis(IServiceCollection services)
        {
            // Read Redis connection string from config or env vars
            var redisConnectionString =
                Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
                ?? "localhost:6379,abortConnect=false";

            // Create a Polly Circuit Breaker policy for Redis
            var redisCircuitBreaker = Policy
                .Handle<RedisConnectionException>()
                .CircuitBreaker(
                    exceptionsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (ex, ts) =>
                    {
                        Console.WriteLine($"Redis circuit OPEN for {ts.TotalSeconds}s due to: {ex.Message}");
                    },
                    onReset: () => Console.WriteLine("Redis circuit CLOSED — connection healthy"),
                    onHalfOpen: () => Console.WriteLine("Redis circuit HALF-OPEN — testing...")
                );

            // Register Redis connection multiplexer with circuit breaker
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                return redisCircuitBreaker.Execute(() =>
                {
                    Console.WriteLine("Connecting to Redis...");
                    return ConnectionMultiplexer.Connect(redisConnectionString);
                });
            });

            // Add other services
            services.AddControllers();
        }

        public static async Task<SqsConfiguration> GetSqsConfigurationAsync(IConfiguration configuration, IWebHostEnvironment environment)
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
