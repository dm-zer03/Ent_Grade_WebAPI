using EcomAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcomAPI.Data.Configurations;

public class ProductCategoryConfiguration
    : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(
        EntityTypeBuilder<ProductCategory> builder)
    {
        // Composite Primary Key
        builder.HasKey(x => new
        {
            x.ProductId,
            x.CategoryId
        });

        // Product → ProductCategory
        builder.HasOne(x => x.Product)
            .WithMany(x => x.ProductCategories)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Category → ProductCategory
        builder.HasOne(x => x.Category)
            .WithMany(x => x.ProductCategories)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}