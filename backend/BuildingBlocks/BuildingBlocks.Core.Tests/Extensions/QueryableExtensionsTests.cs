using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Core.Tests.Extensions;

public class QueryableExtensionsTests
{
    // 1. Tạo Entity và DbContext giả lập mang đậm chất F&B để test
    public class Drink
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<Drink> Drinks { get; set; }
    }

    // Hàm helper để tạo In-Memory DB mới tinh cho mỗi test case (tránh đụng độ data)
    private static TestDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            // Mỗi test dùng 1 tên DB khác nhau (GUID) để đảm bảo cô lập 100%
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    [Fact]
    public async Task ToPagedListAsync_ShouldReturnEmptyResult_WhenNoDataExists()
    {
        // Arrange
        await using var context = CreateInMemoryDbContext();
        var query = context.Drinks.AsQueryable(); // Bảng trống, kích hoạt nhánh (count == 0)

        // Act
        var result = await query.ToPagedListAsync(1, 10);

        // Assert
        result.Should().BeOfType<PagedResult<Drink>>();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task ToPagedListAsync_ShouldReturnFirstPage_WhenDataExists()
    {
        // Arrange
        await using var context = CreateInMemoryDbContext();
        context.Drinks.AddRange(
            new Drink { Id = Guid.NewGuid(), Name = "Cà phê đen đá" },
            new Drink { Id = Guid.NewGuid(), Name = "Bạc xỉu" },
            new Drink { Id = Guid.NewGuid(), Name = "Trà đào cam sả" },
            new Drink { Id = Guid.NewGuid(), Name = "Trà vải" },
            new Drink { Id = Guid.NewGuid(), Name = "Cacao sữa" }
        );
        await context.SaveChangesAsync();

        var query = context.Drinks.OrderBy(d => d.Name).AsQueryable(); // OrderBy để list ổn định

        // Act
        var result = await query.ToPagedListAsync(1, 2); // Lấy trang 1, mỗi trang 2 ly

        // Assert
        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(2);
        // Vì đã OrderBy(Name) nên Bạc xỉu và Cacao sữa sẽ lên đầu
        result.Items[0].Name.Should().Be("Bạc xỉu");
        result.Items[1].Name.Should().Be("Cacao sữa");
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task ToPagedListAsync_ShouldReturnSecondPage_WhenDataExists()
    {
        // Arrange
        await using var context = CreateInMemoryDbContext();
        context.Drinks.AddRange(
            new Drink { Id = Guid.NewGuid(), Name = "Cà phê đen đá" },
            new Drink { Id = Guid.NewGuid(), Name = "Bạc xỉu" },
            new Drink { Id = Guid.NewGuid(), Name = "Trà đào cam sả" },
            new Drink { Id = Guid.NewGuid(), Name = "Trà vải" },
            new Drink { Id = Guid.NewGuid(), Name = "Cacao sữa" }
        );
        await context.SaveChangesAsync();

        var query = context.Drinks.OrderBy(d => d.Name).AsQueryable();

        // Act
        var result = await query.ToPagedListAsync(2, 2); // Lấy trang 2

        // Assert
        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(2);
        // Kiểm tra logic Skip/Take có lấy đúng ly số 3 và 4 không
        result.Items[0].Name.Should().Be("Cà phê đen đá");
        result.Items[1].Name.Should().Be("Trà đào cam sả");
        result.Page.Should().Be(2);
    }

    [Fact]
    public async Task ToPagedListAsync_ShouldReturnRemainingItems_WhenOnLastPageWithOddNumber()
    {
        // Arrange
        await using var context = CreateInMemoryDbContext();
        context.Drinks.AddRange(
            new Drink { Id = Guid.NewGuid(), Name = "Cà phê đen đá" },
            new Drink { Id = Guid.NewGuid(), Name = "Bạc xỉu" },
            new Drink { Id = Guid.NewGuid(), Name = "Trà đào cam sả" },
            new Drink { Id = Guid.NewGuid(), Name = "Trà vải" },
            new Drink { Id = Guid.NewGuid(), Name = "Cacao sữa" }
        );
        await context.SaveChangesAsync();

        var query = context.Drinks.OrderBy(d => d.Name).AsQueryable();

        // Act
        var result = await query.ToPagedListAsync(3, 2); // Lấy trang 3, rớt lại đúng 1 ly cuối cùng

        // Assert
        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("Trà vải");
        result.Page.Should().Be(3);
    }
}