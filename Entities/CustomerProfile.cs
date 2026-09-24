namespace EcomAPI.Entities;

public class CustomerProfile
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    // Navigation property
    public Customer Customer { get; set; } = null!;
}