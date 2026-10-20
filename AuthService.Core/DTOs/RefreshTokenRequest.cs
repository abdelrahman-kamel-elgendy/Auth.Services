using System.ComponentModel.DataAnnotations;

namespace AuthService.Core.DTOs;

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "Refresh token is required.")]
    public string RefreshToken { get; set; } = string.Empty;

    [Required(ErrorMessage = "Access token is required.")]
    public string AccessToken { get; set; } = string.Empty;
}