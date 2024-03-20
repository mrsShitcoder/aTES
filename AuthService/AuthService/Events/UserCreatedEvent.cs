namespace AuthService.Events;

public class UserCreatedEvent
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public IList<string> Roles { get; set; }
}
