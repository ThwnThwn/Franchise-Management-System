using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BuildingBlocks.Core.Tests.Extensions;

public class QueryableExtensionsTests
{
    // 1. Thêm trường Code (không dấu) để OrderBy chuẩn xác trên mọi môi trường OS
    public class Drink
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class TestDbContext(DbContextOptions<QueryableExtensionsTests.TestDbContext> options) : DbContext(options)
    {
        public DbSet<Drink> Drinks { get; set; }
    }

    private static TestDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    [Fact]
    public async Task ToPagedListAsync_ShouldReturnEmptyResult_WhenNoDataExists()
    {
        await using var context = CreateInMemoryDbContext();
        var query = context.Drinks.AsQueryable();

        var result = await query.ToPagedListAsync(1, 10);

        result.Should().BeOfType<PagedResult<Drink>>();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task ToPagedListAsync_ShouldReturnFirstPage_WhenDataExists()
    {
        await using var context = CreateInMemoryDbContext();
        context.Drinks.AddRange(
            new Drink { Id = Guid.NewGuid(), Code = "D01", Name = "Bạc xỉu" },
            new Drink { Id = Guid.NewGuid(), Code = "D02", Name = "Cacao sữa" },
            new Drink { Id = Guid.NewGuid(), Code = "D03", Name = "Cà phê đen đá" },
            new Drink { Id = Guid.NewGuid(), Code = "D04", Name = "Trà đào cam sả" },
            new Drink { Id = Guid.NewGuid(), Code = "D05", Name = "Trà vải" }
        );
        await context.SaveChangesAsync();

        // Order theo Code (D01, D02, D03...) để né lỗi Sort Culture của tiếng Việt
        var query = context.Drinks.OrderBy(d => d.Code).AsQueryable();

        var result = await query.ToPagedListAsync(1, 2);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(2);

        result.Items[0].Name.Should().Be("Bạc xỉu"); // D01
        result.Items[1].Name.Should().Be("Cacao sữa"); // D02
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task ToPagedListAsync_ShouldReturnSecondPage_WhenDataExists()
    {
        await using var context = CreateInMemoryDbContext();
        context.Drinks.AddRange(
            new Drink { Id = Guid.NewGuid(), Code = "D01", Name = "Bạc xỉu" },
            new Drink { Id = Guid.NewGuid(), Code = "D02", Name = "Cacao sữa" },
            new Drink { Id = Guid.NewGuid(), Code = "D03", Name = "Cà phê đen đá" },
            new Drink { Id = Guid.NewGuid(), Code = "D04", Name = "Trà đào cam sả" },
            new Drink { Id = Guid.NewGuid(), Code = "D05", Name = "Trà vải" }
        );
        await context.SaveChangesAsync();

        var query = context.Drinks.OrderBy(d => d.Code).AsQueryable();

        var result = await query.ToPagedListAsync(2, 2); // Lấy trang 2 (Skip 2, Take 2)

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(2);

        result.Items[0].Name.Should().Be("Cà phê đen đá"); // D03
        result.Items[1].Name.Should().Be("Trà đào cam sả"); // D04
        result.Page.Should().Be(2);
    }

    [Fact]
    public async Task ToPagedListAsync_ShouldReturnRemainingItems_WhenOnLastPageWithOddNumber()
    {
        await using var context = CreateInMemoryDbContext();
        context.Drinks.AddRange(
            new Drink { Id = Guid.NewGuid(), Code = "D01", Name = "Bạc xỉu" },
            new Drink { Id = Guid.NewGuid(), Code = "D02", Name = "Cacao sữa" },
            new Drink { Id = Guid.NewGuid(), Code = "D03", Name = "Cà phê đen đá" },
            new Drink { Id = Guid.NewGuid(), Code = "D04", Name = "Trà đào cam sả" },
            new Drink { Id = Guid.NewGuid(), Code = "D05", Name = "Trà vải" }
        );
        await context.SaveChangesAsync();

        var query = context.Drinks.OrderBy(d => d.Code).AsQueryable();

        var result = await query.ToPagedListAsync(3, 2); // Lấy trang 3, rớt lại đúng 1 ly D05

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("Trà vải"); // D05
        result.Page.Should().Be(3);
    }
}