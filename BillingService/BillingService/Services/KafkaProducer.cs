using Confluent.Kafka;

namespace BillingService.Services;

public class KafkaProducer
{
    private string _bootstrapServers;

    public KafkaProducer(string bootstrapServers)
    {
        _bootstrapServers = bootstrapServers;
    }

    public async Task ProduceAsync(string topic, string message)
    {
        var config = new ProducerConfig { BootstrapServers = _bootstrapServers };

        using (var producer = new ProducerBuilder<Null, string>(config).Build())
        {
            await producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
            producer.Flush(TimeSpan.FromSeconds(10));
        }
    }
}