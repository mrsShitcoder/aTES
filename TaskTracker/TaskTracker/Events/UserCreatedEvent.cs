namespace TaskTracker.Events;

public class UserCreatedEvent
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }
}
