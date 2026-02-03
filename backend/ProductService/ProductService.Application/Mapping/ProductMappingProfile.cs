using AutoMapper;
using ProductService.Application.Features.Products.Command.CreateProduct;
using ProductService.Application.Features.Products.Queries.GetById;
using ProductService.Application.Features.Products.Queries.GetByKeyword;
using ProductService.Application.Features.Products.Queries.GetByProductName;
using ProductService.Application.Features.Products.Queries.GetList;
using ProductService.Domain.Entities;

namespace ProductService.Application.Mapping;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<ProductVariant, ProductSizeDto>()
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive ?? true));

        CreateMap<Product, GetProductDetailDto>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : ""))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive ?? true))
            .ForMember(d => d.Sizes, opt => opt.MapFrom(s => s.ProductVariants));

        CreateMap<ProductVariant, ProductSizeListItemDto>()
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive ?? true));

        CreateMap<Product, GetProductListItemDto>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : ""))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive ?? true))
            .ForMember(d => d.Sizes, opt => opt.MapFrom(s => s.ProductVariants));

        CreateMap<CreateProductCommand.CreateProductCommandRequest, Product>()
            .ForMember(d => d.ProductVariants, opt => opt.MapFrom(s => s.Variants));

        CreateMap<CreateProductCommand.CreateProductVariantRequest, ProductVariant>();

        CreateMap<Product, CreateProductCommand.CreateProductCommandResponse>()
            .ForMember(d => d.ProductId, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.VariantIds, opt => opt.MapFrom(s => s.ProductVariants.Select(v => v.Id)));

        CreateMap<UpdateProductCommand.UpdateProductCommandRequest, Product>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.ProductId))
            .ForMember(d => d.ProductVariants, opt => opt.MapFrom(s => s.Variants));

        CreateMap<UpdateProductCommand.UpdateProductVariantRequest, ProductVariant>();

        CreateMap<Product, UpdateProductCommand.UpdateProductCommandResponse>()
            .ForMember(d => d.ProductId, opt => opt.MapFrom(s => s.Id));

        CreateMap<ProductVariant, ProductVariantDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Sku, opt => opt.MapFrom(s => s.Sku))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.Attributes, opt => opt.MapFrom(s => s.Attributes))
            .ForMember(d => d.BasePrice, opt => opt.MapFrom(s => s.BasePrice))
            .ForMember(d => d.Cost, opt => opt.MapFrom(s => s.Cost))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive ?? true));


        CreateMap<Product, GetProductByNameQueryResponse>()
            .ForMember(d => d.ProductId, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description ?? string.Empty))
            .ForMember(d => d.ImageUrl, opt => opt.MapFrom(s => s.ImageUrl ?? string.Empty))
            .ForMember(d => d.ImagePublicId, opt => opt.MapFrom(s => s.ImagePublicId ?? string.Empty))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive ?? true))
            .ForMember(d => d.CategoryId, opt => opt.MapFrom(s => s.CategoryId))
            .ForMember(d => d.CreatedAt, opt => opt.MapFrom(s => s.CreatedAt))
            .ForMember(d => d.UpdatedAt, opt => opt.MapFrom(s => s.UpdatedAt))
            .ForMember(d => d.Variants, opt => opt.MapFrom(s => s.ProductVariants));

        CreateMap<Product, GetProductByKeywordQueryResponse>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
            .ForMember(d => d.ImageUrl, opt => opt.MapFrom(s => s.ImageUrl))
            .ForMember(d => d.ImagePublicId, opt => opt.MapFrom(s => s.ImagePublicId))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive ?? true))
            .ForMember(d => d.CreatedAt, opt => opt.MapFrom(s => s.CreatedAt))
            .ForMember(d => d.UpdatedAt, opt => opt.MapFrom(s => s.UpdatedAt))
            .ForMember(d => d.CategoryId, opt => opt.MapFrom(s => s.CategoryId))
            .ForMember(d => d.Variants, opt => opt.MapFrom(s => s.ProductVariants));

        CreateMap<ProductVariant, ProductVariantDTO>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Sku, opt => opt.MapFrom(s => s.Sku))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.Attributes, opt => opt.MapFrom(s => s.Attributes))
            .ForMember(d => d.BasePrice, opt => opt.MapFrom(s => s.BasePrice))
            .ForMember(d => d.Cost, opt => opt.MapFrom(s => s.Cost))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive ?? true));
    }
}
