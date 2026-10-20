using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Core.Entities;

public class UserPhone
{
    [Key]
    public Guid PhoneId { get; set; }

    public string CountryCode { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool PhoneConfirmed { get; set; } = false;

    public bool IsPrimary { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    [ForeignKey("User")]
    public Guid UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;
}