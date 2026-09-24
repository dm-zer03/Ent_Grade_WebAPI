using AutoMapper;
using AutoMapper.QueryableExtensions;
using EcomAPI.Common.Exceptions;
using EcomAPI.DTOs.Orders;
using EcomAPI.Entities;
using EcomAPI.Repositories.Interfaces;
using EcomAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcomAPI.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<OrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<OrderResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders
            .Query()
            .AsNoTracking()
            .Where(x => x.Id == id)
            .ProjectTo<OrderResponse>(
                _mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            _logger.LogWarning(
                "Order {OrderId} was not found.",
                id);

            return null;
        }

        return order;
    }

    public async Task<IEnumerable<OrderResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders
            .Query()
            .AsNoTracking()
            .ProjectTo<OrderResponse>(
                _mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return orders;
    }

    public async Task<OrderResponse> CreateAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Creating order for CustomerId {CustomerId}.",
            request.CustomerId);

        // 1. Validate customer
        var customer = await _unitOfWork.Customers
            .GetByIdAsync(
                request.CustomerId,
                cancellationToken);

        if (customer is null)
        {
            _logger.LogWarning(
                "Order creation failed. Customer {CustomerId} was not found.",
                request.CustomerId);

            throw new NotFoundException(
                "Customer does not exist.");
        }

        // 2. Validate order items
        if (request.Items is null || request.Items.Count == 0)
        {
            _logger.LogWarning(
                "Order creation failed. Customer {CustomerId} submitted an empty order.",
                request.CustomerId);

            throw new InvalidOperationException(
                "Order must contain at least one item.");
        }

        // 3. Validate quantities before querying products
        foreach (var itemRequest in request.Items)
        {
            if (itemRequest.Quantity <= 0)
            {
                _logger.LogWarning(
                    "Order creation failed. Invalid quantity for ProductId {ProductId}.",
                    itemRequest.ProductId);

                throw new InvalidOperationException(
                    "Quantity must be greater than zero.");
            }
        }

        // 4. Get all required product IDs
        var productIds = request.Items
            .Select(x => x.ProductId)
            .Distinct()
            .ToList();

        // 5. Load all products in ONE database query.
        //    Products remain tracked because stock will be updated.
        var products = await _unitOfWork.Products
            .Query()
            .Where(x => productIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        // Dictionary gives O(1) product lookup inside the loop.
        var productDictionary = products
            .ToDictionary(x => x.Id);

        var order = new Order
        {
            CustomerId = request.CustomerId,
            OrderDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            Status = "Pending"
        };

        decimal totalAmount = 0;

        // 6. Process each item without making database calls
        foreach (var itemRequest in request.Items)
        {
            if (!productDictionary.TryGetValue(
                    itemRequest.ProductId,
                    out var product))
            {
                _logger.LogWarning(
                    "Order creation failed. Product {ProductId} was not found.",
                    itemRequest.ProductId);

                throw new NotFoundException(
                    $"Product {itemRequest.ProductId} does not exist.");
            }

            // 7. Check product status
            if (!product.IsActive)
            {
                _logger.LogWarning(
                    "Order creation failed. Product {ProductId} is inactive.",
                    product.Id);

                throw new InvalidOperationException(
                    $"Product '{product.Name}' is not active.");
            }

            // 8. Check stock
            if (product.StockQuantity < itemRequest.Quantity)
            {
                _logger.LogWarning(
                    "Order creation failed. Insufficient stock for ProductId {ProductId}.",
                    product.Id);

                throw new ConflictException(
                    $"Insufficient stock for product '{product.Name}'.");
            }

            // 9. Get price from database
            var unitPrice = product.Price;

            // 10. Calculate item total
            var itemTotal =
                unitPrice * itemRequest.Quantity;

            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                Quantity = itemRequest.Quantity,
                UnitPrice = unitPrice,
                TotalPrice = itemTotal
            };

            order.OrderItems.Add(orderItem);

            // 11. Reduce stock
            product.StockQuantity -= itemRequest.Quantity;

            // Product is already tracked by EF Core,
            // so Update() is not strictly required.
            _unitOfWork.Products.Update(product);

            // 12. Calculate order total
            totalAmount += itemTotal;
        }

        order.TotalAmount = totalAmount;

        // 13. Add order
        await _unitOfWork.Orders
            .AddAsync(order, cancellationToken);

        // 14. Save everything inside a transaction
        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(
                cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            _logger.LogInformation(
                "Order {OrderId} created successfully for CustomerId {CustomerId}.",
                order.Id,
                order.CustomerId);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(
                cancellationToken);

            _logger.LogWarning(
                ex,
                "Concurrency conflict while creating order for CustomerId {CustomerId}.",
                request.CustomerId);

            throw new ConflictException(
                "The product stock was changed by another request. Please retry.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(
                cancellationToken);

            _logger.LogError(
                ex,
                "Unexpected error while creating order for CustomerId {CustomerId}.",
                request.CustomerId);

            throw;
        }

        return _mapper.Map<OrderResponse>(order);
    }
}
