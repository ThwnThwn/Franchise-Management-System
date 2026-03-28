using EngagementService.Application.Dtos.Campaign.Response;
using EngagementService.Domain.Constants;
using FluentAssertions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EngagementService.Application.Tests.Dtos;

public class CampaignResponseDtoTests
{
    // 1. Test cho class CampaignRes
    [Fact]
    public void CampaignRes_Properties_ShouldSetAndGetCorrectValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var vouchers = new List<Voucher> { new Voucher { Id = Guid.NewGuid(), Code = "PROMO2026" } };

        // Act
        var res = new CampaignRes
        {
            Id = id,
            Name = "Tet Campaign",
            Description = "Big Sale",
            IsActive = true,
            StartDate = now,
            EndDate = now.AddDays(7),
            CreatedAt = now,
            UpdatedAt = now,
            Vouchers = vouchers
        };

        // Assert
        res.Id.Should().Be(id);
        res.Name.Should().Be("Tet Campaign");
        res.Description.Should().Be("Big Sale");
        res.IsActive.Should().BeTrue();
        res.StartDate.Should().Be(now);
        res.EndDate.Should().Be(now.AddDays(7));
        res.Vouchers.Should().HaveCount(1);
        res.Vouchers.First().Code.Should().Be("PROMO2026");
    }

    // 2. Test cho class Voucher với bộ VoucherType mới
    [Fact]
    public void Voucher_Properties_ShouldSetAndGetCorrectValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var voucherItems = new List<VoucherItem> { new VoucherItem { Id = Guid.NewGuid(), Sku = "COFFEE-01" } };

        // Act & Assert: Duyệt qua các loại VoucherType tiêu biểu
        var voucherDiscount = new Voucher
        {
            Id = id,
            Code = "PERCENT50",
            Type = VoucherType.DISCOUNT_PERCENTAGE, // Enum mới
            DiscountValue = 50,
            MaxDiscountValue = 100000,
            MinOrderValue = 200000,
            PointCost = 100,
            InitQuantity = 500,
            CurrentQuantity = 450,
            MaxUsagePerUser = 2,
            IsActive = true,
            VoucherItems = voucherItems
        };

        var voucherShipping = new Voucher
        {
            Id = Guid.NewGuid(),
            Code = "FREESHIP",
            Type = VoucherType.SHIPPING_FIXED_AMOUNT, // Enum mới
            DiscountValue = 15000
        };

        var voucherGift = new Voucher
        {
            Id = Guid.NewGuid(),
            Code = "GIFT_ITEM",
            Type = VoucherType.GIFT // Enum mới
        };

        // Assert
        voucherDiscount.Type.Should().Be(VoucherType.DISCOUNT_PERCENTAGE);
        voucherShipping.Type.Should().Be(VoucherType.SHIPPING_FIXED_AMOUNT);
        voucherGift.Type.Should().Be(VoucherType.GIFT);

        voucherDiscount.Code.Should().Be("PERCENT50");
        voucherDiscount.CurrentQuantity.Should().Be(450);
        voucherDiscount.VoucherItems.Should().HaveCount(1);
    }

    // 3. Test cho class VoucherItem với bộ VoucherItemType mới
    [Fact]
    public void VoucherItem_Properties_ShouldSetAndGetCorrectValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var voucherId = Guid.NewGuid();

        // Act
        var itemGift = new VoucherItem
        {
            Id = id,
            VoucherId = voucherId,
            Sku = "CAFE-SUA",
            Quantity = 1,
            Type = VoucherItemType.ITEM_GIFT // Enum mới 1
        };

        var itemRequired = new VoucherItem
        {
            Id = Guid.NewGuid(),
            VoucherId = voucherId,
            Sku = "BA_KHIA",
            Quantity = 2,
            Type = VoucherItemType.ITEM_REQUIRED // Enum mới 2
        };

        // Assert
        itemGift.Type.Should().Be(VoucherItemType.ITEM_GIFT);
        itemRequired.Type.Should().Be(VoucherItemType.ITEM_REQUIRED);

        itemGift.Sku.Should().Be("CAFE-SUA");
        itemGift.Quantity.Should().Be(1);
        itemRequired.Sku.Should().Be("BA_KHIA");
    }
}