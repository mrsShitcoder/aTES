using System.Text.Json;
using Confluent.Kafka;
using TaskTracker.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TaskTracker.Models;

namespace TaskTracker.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly string _topic;
        private readonly ConsumerConfig _consumerConfig;
        private readonly EventBus _eventBus;

        public KafkaConsumerService(ConsumerConfig config, EventBus eventBus)
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

                    // Предполагаем, что есть поле, указывающее тип сообщения
                    var eventType = messageObject["EventType"].Value<string>();

                    switch (eventType)
                    {
                        case nameof(UserCreatedEvent):
                            var msg = messageObject.ToObject<KafkaMessage<UserCreatedEvent>>();
                            await _eventBus.FireAsync<UserCreatedEvent>(msg.data);
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