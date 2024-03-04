namespace AuthService.Models;

public class UserInfo
{
    public string UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public IList<string> Roles { get; set; }
}
