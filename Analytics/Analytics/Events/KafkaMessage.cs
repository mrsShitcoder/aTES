namespace Analytics.Events;

public class KafkaMessage<TEvent>
{
    public string EventType = nameof(TEvent);
    public  TEvent data { get; set; }
}
