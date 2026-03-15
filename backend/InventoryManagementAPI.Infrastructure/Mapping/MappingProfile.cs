using AutoMapper;
using InventoryManagementAPI.Application.DTOs.Auth;
using InventoryManagementAPI.Application.DTOs.Product;
using InventoryManagementAPI.Domain.Entities;

namespace InventoryManagementAPI.Infrastructure.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserResponse>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

        // Product mappings
        CreateMap<Product, ProductResponse>();
        CreateMap<CreateProductRequest, Product>();
    }
}

