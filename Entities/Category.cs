namespace EcomAPI.Entities;

public class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Many : Many through join entity
    public ICollection<ProductCategory> ProductCategories { get; set; }
        = new List<ProductCategory>();
}