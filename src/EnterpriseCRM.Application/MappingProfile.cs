using AutoMapper;
using EnterpriseCRM.Core.Entities;
using DTOs = EnterpriseCRM.Application.DTOs;

namespace EnterpriseCRM.Application;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Customer, DTOs.CustomerDto>();
        CreateMap<DTOs.CreateCustomerDto, Customer>();
        CreateMap<DTOs.UpdateCustomerDto, Customer>();
        
        CreateMap<User, DTOs.UserDto>();
        CreateMap<DTOs.RegisterUserDto, User>();
        CreateMap<DTOs.UpdateUserDto, User>();

        CreateMap<Product, DTOs.ProductDto>();
        CreateMap<DTOs.CreateProductDto, Product>();
        CreateMap<DTOs.UpdateProductDto, Product>();
    }
}

