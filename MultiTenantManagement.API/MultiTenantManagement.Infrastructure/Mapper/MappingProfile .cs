using AutoMapper;
using MultiTenantManagement.Data.Models;
using MultiTenantManagement.Infrastructure.Features.Product.Dtos;
using MultiTenantManagement.Infrastructure.Features.Tenant.Dtos;


namespace MultiTenantManagement.Infrastructure.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<CreateProductDto, Product>()
                 .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAtUtc, opt => opt.Ignore());
            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.CreatedAtUtc, opt => opt.Ignore());





            CreateMap<CreateTenantRequestDto, Tenant>();
            CreateMap<  UpdateTenantRequestDto, Tenant>();
            CreateMap<TenantDto, Tenant>().ReverseMap();
            CreateMap<StoreSetting, StoreSettingDto>().ReverseMap();
            
        }
    }
}
