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
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.AttachmentUrl, opt => opt.MapFrom(src => src.Attachment != null ? src.Attachment.FileKey : null))
                .ReverseMap()
                .ForMember(dest => dest.Attachment, opt => opt.Ignore());
            CreateMap<CreateProductDto, Product>()
                 .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAtUtc, opt => opt.Ignore());
            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.CreatedAtUtc, opt => opt.Ignore());





            CreateMap<CreateTenantRequestDto, Tenant>();
            CreateMap<  UpdateTenantRequestDto, Tenant>();
            CreateMap<TenantDto, Tenant>()
                .ForMember(dest => dest.Attachment, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.AttachmentUrl, opt => opt.MapFrom(src => src.Attachment != null ? src.Attachment.FileKey : null))
                .ForMember(dest => dest.LogoURL, opt => opt.MapFrom(src => src.Attachment != null ? src.Attachment.FileKey : src.LogoUrl));
            CreateMap<StoreSetting, StoreSettingDto>().ReverseMap();
            
        }
    }
}
