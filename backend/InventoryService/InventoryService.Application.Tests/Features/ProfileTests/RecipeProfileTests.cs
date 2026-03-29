using AutoMapper;
using FluentAssertions;
using InventoryService.Application.Dtos.Recipe;
using InventoryService.Application.Mappers;
using InventoryService.Domain.Entities;

namespace InventoryService.Application.Tests.Mappers;

public class RecipeProfileTests
{
    private readonly IConfigurationProvider _configuration;
    private readonly IMapper _mapper;

    public RecipeProfileTests()
    {
        _configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<RecipeProfile>();
        });
        _mapper = _configuration.CreateMapper();
    }

    // 1. Phủ cấu hình: Kiểm tra tính hợp lệ của mapping (Flattening logic)
    [Fact]
    public void ShouldHaveValidConfiguration()
    {
        _configuration.AssertConfigurationIsValid();
    }

    // 2. Phủ logic Mapping thực tế: Từ Recipe lồng Ingredient sang ViewRes
    [Fact]
    public void Map_Recipe_To_RecipeViewRes_ShouldMapIngredientDetailsCorrectly()
    {
        // Arrange
        var ingredient = new Domain.Entities.Ingredient
        {
            Id = Guid.NewGuid(),
            Name = "Hạt Arabica",
            BaseUnit = "Gram"
        };

        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            IngredientId = ingredient.Id,
            Ingredient = ingredient, // Quan trọng: Phải có object này để map Name/BaseUnit
            Quantity = 15
        };

        // Act
        var result = _mapper.Map<RecipeViewRes>(recipe);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Hạt Arabica");     // Kiểm tra ForMember logic 1
        result.BaseUnit.Should().Be("Gram");      // Kiểm tra ForMember logic 2
    }
}