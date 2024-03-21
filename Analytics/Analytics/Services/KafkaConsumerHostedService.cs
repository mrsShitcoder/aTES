using System.Text.Json;
using Analytics.Events;
using Confluent.Kafka;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Analytics.Models;

namespace Analytics.Services
{
    public class KafkaUserStreamConsumerService : BackgroundService
    {
        private readonly string _topic;
        private readonly ConsumerConfig _consumerConfig;
        private readonly EventBus _eventBus;

        public KafkaUserStreamConsumerService(ConsumerConfig config, EventBus eventBus)
        {
            _topic = "user-stream";
            _consumerConfig = config;
            _eventBus = eventBus;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build())
            {
                consumer.Subscribe(_topic);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(stoppingToken);

                    Console.WriteLine($"Received message: {consumeResult.Message.Value}");

                    var messageObject = JsonConvert.DeserializeObject<JObject>(consumeResult.Message.Value);

                    var eventType = messageObject["EventType"].Value<string>();

                    switch (eventType)
                    {
                        case nameof(UserCreatedEvent):
                            var userCreated = messageObject.ToObject<KafkaMessage<UserCreatedEvent>>();
                            await _eventBus.FireAsync<UserCreatedEvent>(userCreated.data);
                            break;
                        default:
                            Console.WriteLine($"Unknown event type {eventType}");
                            break;
                    }

                    consumer.Commit(consumeResult);
                }

                consumer.Close();
            }
        }
    }
    
    public class KafkaTaskStreamConsumerService : BackgroundService
    {
        private readonly string _topic;
        private readonly ConsumerConfig _consumerConfig;
        private readonly EventBus _eventBus;

        public KafkaTaskStreamConsumerService(ConsumerConfig config, EventBus eventBus)
        {
            _topic = "task-stream";
            _consumerConfig = config;
            _eventBus = eventBus;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build())
            {
                consumer.Subscribe(_topic);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(stoppingToken);

                    Console.WriteLine($"Received message: {consumeResult.Message.Value}");

                    var messageObject = JsonConvert.DeserializeObject<JObject>(consumeResult.Message.Value);

                    var eventType = messageObject["EventType"].Value<string>();

                    switch (eventType)
                    {
                        case nameof(TaskCreatedEvent):
                            var taskCreated = messageObject.ToObject<KafkaMessage<TaskCreatedEvent>>();
                            await _eventBus.FireAsync<TaskCreatedEvent>(taskCreated.data);
                            break;
                        default:
                            Console.WriteLine($"Unknown event type {eventType}");
                            break;
                    }

                    consumer.Commit(consumeResult);
                }

                consumer.Close();
            }
        }
    }
    
    public class KafkaTaskEventsConsumerService : BackgroundService
    {
        private readonly string _topic;
        private readonly ConsumerConfig _consumerConfig;
        private readonly EventBus _eventBus;

        public KafkaTaskEventsConsumerService(ConsumerConfig config, EventBus eventBus)
        {
            _topic = "task-events";
            _consumerConfig = config;
            _eventBus = eventBus;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build())
            {
                consumer.Subscribe(_topic);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(stoppingToken);

                    Console.WriteLine($"Received message: {consumeResult.Message.Value}");

                    var messageObject = JsonConvert.DeserializeObject<JObject>(consumeResult.Message.Value);

                    var eventType = messageObject["EventType"].Value<string>();

                    switch (eventType)
                    {
                        case nameof(TaskCompletedEvent):
                            var taskCompleted = messageObject.ToObject<KafkaMessage<TaskCompletedEvent>>();
                            await _eventBus.FireAsync<TaskCompletedEvent>(taskCompleted.data);
                            break;
                        case nameof(TaskPriceUpdatedEvent):
                            var taskPriceUpdated = messageObject.ToObject<KafkaMessage<TaskPriceUpdatedEvent>>();
                            await _eventBus.FireAsync(taskPriceUpdated.data);
                            break;
                        default:
                            Console.WriteLine($"Unknown event type {eventType}");
                            break;
                    }

                    consumer.Commit(consumeResult);
                }

                consumer.Close();
            }
        }
    }
    
    public class KafkaAccountEventsConsumerService : BackgroundService
    {
        private readonly string _topic;
        private readonly ConsumerConfig _consumerConfig;
        private readonly EventBus _eventBus;

        public KafkaAccountEventsConsumerService(ConsumerConfig config, EventBus eventBus)
        {
            _topic = "account-events";
            _consumerConfig = config;
            _eventBus = eventBus;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build())
            {
                consumer.Subscribe(_topic);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(stoppingToken);

                    Console.WriteLine($"Received message: {consumeResult.Message.Value}");

                    var messageObject = JsonConvert.DeserializeObject<JObject>(consumeResult.Message.Value);

                    var eventType = messageObject["EventType"].Value<string>();

                    switch (eventType)
                    {
                        case nameof(AccountBalanceChangedEvent):
                            var taskCompleted = messageObject.ToObject<KafkaMessage<AccountBalanceChangedEvent>>();
                            await _eventBus.FireAsync<AccountBalanceChangedEvent>(taskCompleted.data);
                            break;
                        default:
                            Console.WriteLine($"Unknown event type {eventType}");
                            break;
                    }

                    consumer.Commit(consumeResult);
                }

                consumer.Close();
            }
        }
    }
}