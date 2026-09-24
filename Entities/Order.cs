namespace EcomAPI.Entities;

public class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public DateTime OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public Customer Customer { get; set; } = null!;

    // 1 : Many
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}