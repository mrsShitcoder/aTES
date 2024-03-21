using System.ComponentModel.DataAnnotations;

namespace AuthService.Models;

public class LoginInfo
{
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; }
    
    [Required]
    public string Password { get; set; }
}
