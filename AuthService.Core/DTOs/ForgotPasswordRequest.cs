using System.ComponentModel.DataAnnotations;

namespace AuthService.Core.DTOs;

public class ForgotPasswordRequest
{
    [Required(ErrorMessage = "Email is required!")]
    [EmailAddress(ErrorMessage = "Invalid email format!")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters!")]
    public string Email { get; set; } = string.Empty;
}