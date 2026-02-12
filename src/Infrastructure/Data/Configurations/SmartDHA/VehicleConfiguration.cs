using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DHAFacilitationAPIs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DHAFacilitationAPIs.Infrastructure.Data.Configurations.SmartDHA;
public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        {
            builder.ToTable("Vehicles");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.LicenseNo)
                  .HasMaxLength(50)
                  .IsRequired();

            builder.Property(e => e.Make)
                  .HasMaxLength(100);

            builder.Property(e => e.Model)
                  .HasMaxLength(100);

            builder.Property(e => e.Year)
                  .HasMaxLength(4)
                  .IsRequired();

            builder.Property(e => e.Color)
                  .HasMaxLength(50)
                  .IsRequired();

            builder.Property(e => e.Attachment)
                  .HasColumnType("nvarchar(max)");

            // Relationship with ApplicationUser
            builder.HasOne(e => e.ApplicationUser)
                  .WithMany(v => v.Vehicles)
                  .HasForeignKey(e => e.ApplicationUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            builder.Property(e => e.Created)
                  .HasColumnType("datetimeoffset")
                  .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            builder.Property(e => e.LastModified)
                  .HasColumnType("datetimeoffset");

            builder.Property(e => e.CreatedBy)
                  .HasMaxLength(100);

            builder.Property(e => e.LastModifiedBy)
                  .HasMaxLength(100);

            builder.Property(e => e.IsActive)
                  .HasDefaultValue(true);

            builder.Property(e => e.IsDeleted)
                  .HasDefaultValue(false);

        }
    }

}
