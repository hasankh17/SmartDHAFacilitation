using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DHAFacilitationAPIs.Application.Common.Interfaces;
using DHAFacilitationAPIs.Domain.Common;
using DHAFacilitationAPIs.Domain.Entities.SmartDHA;
using Microsoft.EntityFrameworkCore;

namespace DHAFacilitationAPIs.Infrastructure.Data;
public class SmartdhaDbContext : DbContext, ISmartdhaDbContext
{
    private readonly IUser _loggedInUser;
    public SmartdhaDbContext(DbContextOptions<SmartdhaDbContext> options, IUser loggedInUser) : base(options)
    {
        _loggedInUser = loggedInUser;
    }

    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Property> Properties { get; set; }
    public DbSet<UserFamily> UserFamilies { get; set; }

    public override async Task<int> SaveChangesAsync(
      CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;

        foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.Created = now;
                entry.Entity.CreatedBy = _loggedInUser.Id;
                entry.Entity.IsActive = true;
                entry.Entity.IsDeleted = false;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.LastModified = now;
                entry.Entity.LastModifiedBy = _loggedInUser.Id;
            }

            // Soft Delete support
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.IsActive = false;
                entry.Entity.LastModified = now;
                entry.Entity.LastModifiedBy = _loggedInUser.Id;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply configurations if you use IEntityTypeConfiguration<>
        modelBuilder.ApplyConfigurationsFromAssembly(
        typeof(SmartdhaDbContext).Assembly,
        t => t.Namespace!.Contains("Data.Configurations.SmartDHA")
    );
    
        base.OnModelCreating(modelBuilder);
    }
}
