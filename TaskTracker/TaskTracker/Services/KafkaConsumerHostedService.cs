using Confluent.Kafka;

namespace TaskTracker.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly string _topic;
        private readonly ConsumerConfig _consumerConfig;

        public KafkaConsumerService(ConsumerConfig config)
        {
            _topic = "user-stream";
            _consumerConfig = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build())
            {
                consumer.Subscribe(_topic);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(stoppingToken);

                    // Process the message
                    Console.WriteLine($"Received message: {consumeResult.Message.Value}");

                    // Commit the offset of the consumed message
                    consumer.Commit(consumeResult);
                }

                consumer.Close();
            }
        }
    }
}