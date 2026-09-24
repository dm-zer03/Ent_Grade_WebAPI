using EcomAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcomAPI.Data.Configurations;

public class CustomerProfileConfiguration
    : IEntityTypeConfiguration<CustomerProfile>
{
    public void Configure(EntityTypeBuilder<CustomerProfile> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Address)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.State)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.PostalCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.DateOfBirth)
            .IsRequired();

        builder.HasIndex(x => x.CustomerId)
            .IsUnique();
    }
}