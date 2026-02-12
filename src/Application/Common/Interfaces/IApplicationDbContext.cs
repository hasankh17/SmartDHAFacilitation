

using DHAFacilitationAPIs.Domain.Entities;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DHAFacilitationAPIs.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Property> Properties { get; set; }
    public DbSet<UserFamily> UserFamilies { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    DatabaseFacade Database { get; }
}
