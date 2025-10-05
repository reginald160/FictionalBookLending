using Amazon.SQS;
using Amazon.SQS.Model;
using FictionalBookLending.src.Application.Abstractions.Events;
using FictionalBookLending.src.Domain.Abstractions;
using FictionalBookLending.src.Infrastructure.Configuration;
using System.Text.Json;

namespace FictionalBookLending.src.Infrastructure.Messages.AWS
{
    public sealed class SQSEventPublishert : IEventPublisher
    {
        private readonly IAmazonSQS _sqs;
        private readonly string _queueUrl;
        private readonly SqsConfiguration _sqsConfiguration;
        private readonly IConfiguration _cfg;

        public SQSEventPublishert(IAmazonSQS sqs, IConfiguration cfg, SqsConfiguration sqsConfiguration)
        {
            _sqs = sqs;
            _cfg = cfg;
            _sqsConfiguration=sqsConfiguration;
            _queueUrl = cfg["QueueType:Type"] == "FIFO" ? _sqsConfiguration.FifoQueueUrl : _sqsConfiguration.MainQueueUrl; // resolved from env for LocalStack/AWS
            //_sqsConfiguration=sqsConfiguration;
        }

        public async Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken ct)
        {
           //var sss = await  SqsConfiguration.GetSqsConfigurationAsync(cfg)
            
            foreach (var e in events)
            {
                var body = JsonSerializer.Serialize(e);

                // Convert event attributes into SQS message attributes
                var attributes = new Dictionary<string, MessageAttributeValue>
                {
                    ["EventType"] = new() { DataType = "String", StringValue = e.EventType },
                    ["Message"] = new() { DataType = "String", StringValue = e.Message }
                };

                foreach (var (key, value) in e.Attributes)
                {
                    attributes[key] = new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = value
                    };
                }

                await _sqs.SendMessageAsync(new SendMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MessageBody = body,
                    MessageAttributes = attributes
                }, ct);
            }
        }
    }
}
