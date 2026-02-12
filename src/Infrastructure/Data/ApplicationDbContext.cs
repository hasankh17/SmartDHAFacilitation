using System.Reflection;
using DHAFacilitationAPIs.Application.Common.Interfaces;
using DHAFacilitationAPIs.Application.Common.Models;
using DHAFacilitationAPIs.Domain.Entities.SmartDHA;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ApplicationUser = DHAFacilitationAPIs.Domain.Entities.ApplicationUser;

namespace DHAFacilitationAPIs.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

   
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        IdentityBuilder(builder);
        //builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        builder.ApplyConfigurationsFromAssembly(
        typeof(ApplicationDbContext).Assembly,
        t => t.Namespace!.Contains("Data.SmartDHA"));
    }
    
    private static void IdentityBuilder(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>(
            entity =>
            {
                entity.ToTable("Users");
                entity.Property(e => e.Id).HasMaxLength(85);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.NormalizedEmail).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.NormalizedUserName).HasMaxLength(100);
            });

        builder.Entity<IdentityRole>(
            entity =>
            {
                entity.ToTable("Role");
                entity.Property(e => e.Id).HasMaxLength(85);
                entity.Property(e => e.NormalizedName).HasMaxLength(100);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.ConcurrencyStamp).HasMaxLength(85);
            });

        builder.Entity<IdentityUserRole<Guid>>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.Property(e => e.UserId).HasMaxLength(85);
            entity.Property(e => e.RoleId).HasMaxLength(85);
        });

        builder.Entity<IdentityUserClaim<Guid>>(entity =>
        {
            entity.ToTable("UserClaims");
            entity.Property(e => e.Id).HasMaxLength(85);
            entity.Property(e => e.UserId).HasMaxLength(85);
        });

        builder.Entity<IdentityUserLogin<Guid>>(entity =>
        {
            entity.ToTable("UserLogins");
            entity.Property(e => e.LoginProvider).HasMaxLength(85);
            entity.Property(e => e.ProviderKey).HasMaxLength(85);
            entity.Property(e => e.UserId).HasMaxLength(85);
        });

        builder.Entity<IdentityUserToken<Guid>>(entity =>
        {
            entity.ToTable("UserTokens").HasKey(e => new { e.UserId, e.LoginProvider });
            entity.Property(e => e.UserId).HasMaxLength(85);
        });

        builder.Entity<IdentityRoleClaim<Guid>>(entity =>
        {
            entity.ToTable("RoleClaims");
            entity.Property(e => e.Id).HasMaxLength(85);
            entity.Property(e => e.RoleId).HasMaxLength(85);
        });

    }
}
