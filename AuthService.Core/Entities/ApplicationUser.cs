using Microsoft.AspNetCore.Identity;

namespace AuthService.Core.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;


    public string? ProfileUrl { get; set; }
    public DateTime? BirthDate { get; set; }

    public bool IsBlocked { get; set; }
    public DateTime? BlockedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }


    // Navigation properties
    public virtual ICollection<UserPhone> UserPhones { get; set; } = new List<UserPhone>();
    public virtual ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}