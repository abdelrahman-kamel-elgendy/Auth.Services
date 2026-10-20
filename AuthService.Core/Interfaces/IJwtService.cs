using AuthService.Core.Entities;
using System.Security.Claims;

namespace AuthService.Core.Interfaces
{
    public interface IJwtService
    {
        Task<string> GenerateAccessToken(ApplicationUser user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}