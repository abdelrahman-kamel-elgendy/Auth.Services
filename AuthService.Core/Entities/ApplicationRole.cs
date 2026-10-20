using Microsoft.AspNetCore.Identity;

namespace AuthService.Core.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public string? NormalizedUsername { get; set; }
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}