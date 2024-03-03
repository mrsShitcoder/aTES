using System.ComponentModel.DataAnnotations;

namespace AuthService.Models;

public class RegInfo
{
    [Required]
    public string Name { get; set; }
    [Required]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; }
    
    [Required]
    public string Password { get; set; }
}