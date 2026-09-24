namespace EcomAPI.Entities;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public DateTime? UpdatedAt { get; set; }

    // 1 : Many
    public ICollection<OrderItem> OrderItems { get; set; }
        = new List<OrderItem>();

    // Many : Many through join entity
    public ICollection<ProductCategory> ProductCategories { get; set; }
        = new List<ProductCategory>();
}