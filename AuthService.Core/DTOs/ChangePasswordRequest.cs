using System.ComponentModel.DataAnnotations;

namespace AuthService.Core.DTOs;

public class ChangePasswordRequest
{
    [Required(ErrorMessage = "Current password is required!")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required!")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters!")]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must have at least 1 uppercase, 1 lowercase, 1 number, and 1 special character.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}