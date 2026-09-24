
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EcomAPI.Common.Exceptions;
using EcomAPI.DTOs.Customers;
using EcomAPI.Entities;
using EcomAPI.Repositories.Interfaces;
using EcomAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcomAPI.Services.Implementations;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CustomerService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CustomerResponse?> GetByIdAsync(
        int id,
        int currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers
            .GetByIdAsync(id, cancellationToken);

        if (customer is null)
        {
            return null;
        }

        // Admin can access any customer.
        // Customer can access only their own record.
        if (!isAdmin && customer.UserId != currentUserId)
        {
            _logger.LogWarning(
                "User {UserId} attempted to access customer {CustomerId} without permission.",
                currentUserId,
                id);

            throw new ForbiddenException(
                "You are not allowed to access this customer.");
        }

        return _mapper.Map<CustomerResponse>(customer);
    }

    public async Task<IEnumerable<CustomerResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var customers = await _unitOfWork.Customers
            .Query()
            .AsNoTracking()
            .ProjectTo<CustomerResponse>(
                _mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return customers;
    }

    public async Task<CustomerResponse> CreateAsync(
        CreateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var emailExists = await _unitOfWork.Customers
            .ExistsAsync(
                x => x.Email == request.Email,
                cancellationToken);

        if (emailExists)
        {
            _logger.LogWarning(
                "Customer creation failed because the email already exists.");

            throw new ConflictException(
                "A customer with this email already exists.");
        }

        var customer = _mapper.Map<Customer>(request);

        customer.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Customers
            .AddAsync(customer, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Customer {CustomerId} created successfully.",
            customer.Id);

        return _mapper.Map<CustomerResponse>(customer);
    }

    public async Task<CustomerResponse?> UpdateAsync(
        int id,
        UpdateCustomerRequest request,
        int currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers
            .GetByIdAsync(id, cancellationToken);

        if (customer is null)
        {
            return null;
        }

        // Admin can update any customer.
        // Customer can update only their own record.
        if (!isAdmin && customer.UserId != currentUserId)
        {
            _logger.LogWarning(
                "User {UserId} attempted to update customer {CustomerId} without permission.",
                currentUserId,
                id);

            throw new ForbiddenException(
                "You are not allowed to update this customer.");
        }

        var emailExists = await _unitOfWork.Customers
            .ExistsAsync(
                x => x.Email == request.Email &&
                     x.Id != id,
                cancellationToken);

        if (emailExists)
        {
            _logger.LogWarning(
                "Customer update failed for customer {CustomerId} because the email already exists.",
                id);

            throw new ConflictException(
                "A customer with this email already exists.");
        }

        _mapper.Map(request, customer);

        customer.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Customers.Update(customer);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Customer {CustomerId} updated successfully.",
            customer.Id);

        return _mapper.Map<CustomerResponse>(customer);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers
            .GetByIdAsync(id, cancellationToken);

        if (customer is null)
        {
            return false;
        }

        _unitOfWork.Customers.Delete(customer);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Customer {CustomerId} deleted successfully.",
            customer.Id);

        return true;
    }
}
