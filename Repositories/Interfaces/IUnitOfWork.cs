using EcomAPI.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace EcomAPI.Repositories.Interfaces;

public interface IUnitOfWork
{
    IRepository<Customer> Customers { get; }

    IRepository<Product> Products { get; }

    IRepository<User> Users { get; }

    IRepository<Category> Categories { get; }

    IRepository<Order> Orders { get; }

    IRepository<OrderItem> OrderItems { get; }

    IRepository<CustomerProfile> CustomerProfiles { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default);



}