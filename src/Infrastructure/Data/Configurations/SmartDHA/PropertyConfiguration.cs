using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DHAFacilitationAPIs.Domain.Entities.SmartDHA;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Dapper.SqlMapper;

namespace DHAFacilitationAPIs.Infrastructure.Data.Configurations.SmartDHA;
public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        {
            builder.ToTable("Properties");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Category)
                  .HasConversion<int>();

            builder.Property(e => e.Type)
                  .HasConversion<int>();

            builder.Property(e => e.Phase)
                  .HasConversion<int>();

            builder.Property(e => e.Zone)
                  .HasConversion<int>();

            builder.Property(e => e.PossessionType)
                  .HasConversion<int>()
                  .IsRequired();

            builder.Property(e => e.Khayaban)
                  .HasMaxLength(150);

                builder.Property(e => e.StreetNo)
                  .HasMaxLength(50);

            builder.Property(e => e.Plot)
                  .HasMaxLength(50);

            builder.Property(e => e.PlotNo)
                  .IsRequired();

            builder.Property(e => e.Floor)
                  .IsRequired();

            builder.Property(e => e.ProofOfPossessionImage)
                  .HasColumnType("nvarchar(max)");

            builder.Property(e => e.UtilityBillAttachment)
                  .HasColumnType("nvarchar(max)");

            // Relationship with ApplicationUser (if properties belong to users)
            builder.HasOne(e => e.ApplicationUser)
                  .WithMany(u => u.Properties)
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
