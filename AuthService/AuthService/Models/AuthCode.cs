namespace AuthService.Models;

public class AuthCode
{
    public string ClientId { get; set; }
    public string CodeChallenge { get; set; }
    public string CodeChallengeMethod { get; set; }
    public string RedirectUrl { get; set; }
    public DateTime Expiry { get; set; }
}