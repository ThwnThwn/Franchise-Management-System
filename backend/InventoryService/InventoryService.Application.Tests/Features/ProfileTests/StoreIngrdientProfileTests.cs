using AutoMapper;
using InventoryService.Application.Dtos.StoreIngredient;
using InventoryService.Application.Mappers;
using InventoryService.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace InventoryService.Application.Tests.Mappers;

public class StoreIngredientProfileTests
{
    private readonly IConfigurationProvider _configuration;
    private readonly IMapper _mapper;

    public StoreIngredientProfileTests()
    {
        _configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<StoreIngredientProfile>();
            // Ignore Ingredient để pass config check
            cfg.CreateMap<StoreIngredient, StockViewRes>()
               .ForMember(dest => dest.Ingredient, opt => opt.Ignore());
        });
        _mapper = _configuration.CreateMapper();
    }

    [Theory]
    [InlineData(10, 5, 0)] // > Threshold
    [InlineData(0, 5, 2)]  // == 0
    [InlineData(3, 5, 1)]  // 0 < Qty <= Threshold
    public void Map_ShouldReturnCorrectStockStatus(decimal qty, decimal threshold, int expectedStatus)
    {
        var src = new StoreIngredient { Quantity = qty, MinThreshold = threshold };
        var res = _mapper.Map<StockViewRes>(src);
        ((int)res.StockStatus).Should().Be(expectedStatus);
    }
}