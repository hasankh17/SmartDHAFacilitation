using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DHAFacilitationAPIs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Dapper.SqlMapper;

namespace DHAFacilitationAPIs.Infrastructure.Data.Configurations.SmartDHA;
public class UserFamilyConfiguration : IEntityTypeConfiguration<UserFamily>
{
    public void Configure(EntityTypeBuilder<UserFamily> builder)
    {
        {
            builder.ToTable("UserFamilies");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name)
                  .HasMaxLength(150)
                  .IsRequired();

            builder.Property(e => e.ResidentCardNumber)
                  .HasMaxLength(200)
                  .IsRequired();

            builder.Property(e => e.Cnic)
                  .HasMaxLength(15);

            builder.Property(e => e.PhoneNumber)
                  .HasMaxLength(20);

            builder.Property(e => e.FatherOrHusbandName)
                  .HasMaxLength(150);

            builder.Property(e => e.Relation)
                  .HasMaxLength(50);

            builder.Property(e => e.ProfilePicture)
                  .HasColumnType("nvarchar(max)");

            builder.Property(e => e.DateOfBirth)
                  .HasColumnType("datetime2")
                  .IsRequired();

            // Relationship with ApplicationUser
            builder.HasOne(e => e.ApplicationUser)
                  .WithMany(u => u.UserFamilies)
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
