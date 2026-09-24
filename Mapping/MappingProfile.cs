using AutoMapper;
using EcomAPI.DTOs.Categories;
using EcomAPI.DTOs.Customers;
using EcomAPI.DTOs.Products;
using EcomAPI.Entities;
using EcomAPI.DTOs.Orders;

namespace EcomAPI.Mapping;

public class MappingProfile : Profile
{
   
        public MappingProfile()
        {
            // Customer
            CreateMap<CreateCustomerRequest, Customer>();
            CreateMap<UpdateCustomerRequest, Customer>();
            CreateMap<Customer, CustomerResponse>();

            // Product
            CreateMap<CreateProductRequest, Product>();
            CreateMap<UpdateProductRequest, Product>();
            CreateMap<Product, ProductResponse>();

            // Category
            CreateMap<CreateCategoryRequest, Category>();
            CreateMap<UpdateCategoryRequest, Category>();
            CreateMap<Category, CategoryResponse>();

            // Order
            CreateMap<CreateOrderItemRequest, OrderItem>();
            CreateMap<OrderItem, OrderItemResponse>();

            CreateMap<Order, OrderResponse>()
                .ForMember(
                    dest => dest.Items,
                    opt => opt.MapFrom(src => src.OrderItems));
        }
}
