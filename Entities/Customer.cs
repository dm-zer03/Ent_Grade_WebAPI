namespace EcomAPI.Entities;

public class Customer
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // 1 : 1
    public CustomerProfile? Profile { get; set; }

    // 1 : 1 with User
    public int UserId { get; set; }

    public User? User { get; set; }


    // 1 : Many
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}