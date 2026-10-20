using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AuthService.Core.Entities;

namespace AuthService.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserPhone> UserPhones { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // UserPhone configuration
            builder.Entity<UserPhone>(entity =>
            {
                entity.HasKey(e => e.PhoneId);
                entity.Property(e => e.CountryCode).IsRequired().HasMaxLength(10);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);

                entity.HasOne(up => up.User)
                    .WithMany(u => u.UserPhones)
                    .HasForeignKey(up => up.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // UserAddress configuration
            builder.Entity<UserAddress>(entity =>
            {
                entity.HasKey(e => e.AddressId);
                entity.Property(e => e.AddressLine1).IsRequired().HasMaxLength(500);
                entity.Property(e => e.City).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Country).IsRequired().HasMaxLength(500);

                entity.HasOne(ua => ua.User)
                    .WithMany(u => u.UserAddresses)
                    .HasForeignKey(ua => ua.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // RefreshToken configuration
            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.TokenId);
                entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
                entity.Property(e => e.CreatedByIp).IsRequired().HasMaxLength(50);
                entity.Property(e => e.RevokedByIp).HasMaxLength(50);

                entity.HasOne(rt => rt.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Table names to match your schema
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<ApplicationRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        }
    }
}