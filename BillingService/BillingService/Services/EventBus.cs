namespace BillingService.Services;

public class EventBus
{
    private Dictionary<Type, List<Delegate>> _listeners;

    public void Subscribe<TEvent>(Func<TEvent, Task> action)
    {
        var eventType = typeof(TEvent);
        if (!_listeners.ContainsKey(eventType))
        {
            _listeners[eventType] = new List<Delegate>();
        }
        _listeners[eventType].Add(action);
    }

    public async Task FireAsync<TEvent>(TEvent eventData)
    {
        var eventType = typeof(TEvent);
        if (_listeners.TryGetValue(eventType, out var listeners))
        {
            foreach (var action in listeners)
            {
                await ((Func<TEvent, Task>)action)(eventData);
            }
        }
    }
}
