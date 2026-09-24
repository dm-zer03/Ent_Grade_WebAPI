using EcomAPI.Data;
using EcomAPI.Entities;
using EcomAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace EcomAPI.Repositories.Implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    private IRepository<User>? _Users;

    private IRepository<Customer>? _customers;
    private IRepository<Product>? _products;
    private IRepository<Category>? _categories;
    private IRepository<Order>? _orders;
    private IRepository<OrderItem>? _orderItems;
    private IRepository<CustomerProfile>? _customerProfiles;

    public IRepository<User> Users =>
        _Users ??= new Repository<User>(_context);

    public IRepository<Customer> Customers =>
        _customers ??= new Repository<Customer>(_context);

    public IRepository<Product> Products =>
        _products ??= new Repository<Product>(_context);

    public IRepository<Category> Categories =>
        _categories ??= new Repository<Category>(_context);

    public IRepository<Order> Orders =>
        _orders ??= new Repository<Order>(_context);

    public IRepository<OrderItem> OrderItems =>
        _orderItems ??= new Repository<OrderItem>(_context);

    public IRepository<CustomerProfile> CustomerProfiles =>
        _customerProfiles ??=
            new Repository<CustomerProfile>(_context);

    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(
    CancellationToken cancellationToken = default)
    {
        return await _context.Database
            .BeginTransactionAsync(cancellationToken);
    }
}