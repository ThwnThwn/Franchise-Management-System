using FluentAssertions;
using ProductService.Application.Dtos;
using ProductService.Application.Mapping;
using ProductService.Domain.Entities;

namespace ProductService.Application.Tests.Mapping;

public class ProductMappingExtentionsTests
{
    [Fact]
    public void MapToProductDto_ShouldMapAllProperties_WhenNavigationPropertiesArePresent()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Trà Đào Cam Sả",
            Description = "Thanh mát, đậm vị trà",
            ImageUrl = "https://quancaphe.com/tra-dao.png",
            IsActive = true,
            IsDeleted = false,
            CategoryId = Guid.NewGuid(),
            Category = new Category { Name = "Trà Trái Cây" }, // Có danh mục
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            ProductVariants = new List<ProductVariant> // Có danh sách size
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Sku = "TDC-SIZE-M",
                    Name = "Size M",
                    Attributes = "Ice: 50%, Sugar: 50%",
                    BasePrice = 45000m,
                    Cost = 20000m,
                    IsActive = true
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Sku = "TDC-SIZE-L",
                    Name = "Size L",
                    Attributes = "Ice: 50%, Sugar: 50%",
                    BasePrice = 55000m,
                    Cost = 25000m,
                    IsActive = true
                }
            }
        };

        // Act
        var result = product.MapToProductDto();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ProductDto>();

        // Kiểm tra thông tin cơ bản
        result.Id.Should().Be(product.Id);
        result.Name.Should().Be(product.Name);
        result.Description.Should().Be(product.Description);
        result.ImageUrl.Should().Be(product.ImageUrl);
        result.IsActive.Should().Be(product.IsActive);
        result.IsDeleted.Should().Be(product.IsDeleted);
        result.CategoryId.Should().Be(product.CategoryId);
        result.CreatedAt.Should().Be(product.CreatedAt);
        result.UpdatedAt.Should().Be(product.UpdatedAt);

        // Kiểm tra mapping Category (Trường hợp không Null)
        result.CategoryName.Should().Be("Trà Trái Cây");

        // Kiểm tra mapping Variants (Trường hợp không Null)
        result.Variants.Should().NotBeNull();
        result.Variants.Should().HaveCount(2);

        var firstVariantDto = result.Variants.First(v => v.Id == product.ProductVariants.First().Id);
        var firstExpectedVariant = product.ProductVariants.First();

        firstVariantDto.Sku.Should().Be(firstExpectedVariant.Sku);
        firstVariantDto.Name.Should().Be(firstExpectedVariant.Name);
        firstVariantDto.Attributes.Should().Be(firstExpectedVariant.Attributes);
        firstVariantDto.BasePrice.Should().Be(firstExpectedVariant.BasePrice);
        firstVariantDto.Cost.Should().Be(firstExpectedVariant.Cost);
        firstVariantDto.IsActive.Should().Be(firstExpectedVariant.IsActive);
    }

    [Fact]
    public void MapToProductDto_ShouldHandleNullNavigationProperties_WhenCategoryAndVariantsAreNull()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Cà phê đen đá",
            Description = "Cà phê phin truyền thống",
            CategoryId = Guid.NewGuid(),
            // CỐ TÌNH ĐỂ NULL để test Fallback logic của bro
            Category = null!,
            ProductVariants = null!
        };

        // Act
        var result = product.MapToProductDto();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Cà phê đen đá");

        // ĐÂY LÀ PHẦN QUAN TRỌNG NHẤT: Kiểm tra xem toán tử ?? có hoạt động chuẩn không
        result.CategoryName.Should().Be(string.Empty, "Vì Category bị null nên CategoryName phải gán bằng string.Empty");

        result.Variants.Should().NotBeNull("Vì Variants bị null nên phải fallback về list rỗng []");
        result.Variants.Should().BeEmpty();
    }
}