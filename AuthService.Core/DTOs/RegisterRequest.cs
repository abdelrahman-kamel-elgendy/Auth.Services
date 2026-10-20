using System.ComponentModel.DataAnnotations;
using AuthService.Core.Validators;

namespace AuthService.Core.DTOs;

public class RegisterRequest
{
    [Required(ErrorMessage = "First name is required!")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters!")]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(50, MinimumLength = 2, ErrorMessage = "Middle name must be between 2 and 50 characters!")]
    public string? MiddleName { get; set; }

    [Required(ErrorMessage = "Last name is required!")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters!")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Username is required!")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters!")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required!")]
    [EmailAddress(ErrorMessage = "Invalid email format!")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters!")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required!")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters!")]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must have at least 1 uppercase, 1 lowercase, 1 number, and 1 special character.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Url(ErrorMessage = "Invalid URL format!")]
    [StringLength(200, ErrorMessage = "Profile URL cannot exceed 200 characters!")]
    public string? ProfileURL { get; set; }

    [DataType(DataType.Date)]
    [CustomValidation(typeof(DateValidator), nameof(DateValidator.Validate))]
    public DateTime? BirthDate { get; set; }
}