using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AuthService.Core.Entities
{
    public class UserAddress
    {
        [Key]
        public Guid AddressId { get; set; }

        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string? State { get; set; }
        public string Country { get; set; } = string.Empty;
        public string? PostalCode { get; set; }

        public bool IsDefault { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        [ForeignKey("User")]
        public Guid UserId { get; set; }
        [JsonIgnore]
        public virtual ApplicationUser User { get; set; } = null!;
    }
}