using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Pagination;
using FluentAssertions;

namespace BuildingBlocks.Core.Tests.Extensions;

public class ListExtensionTests
{
    [Fact]
    public void ToPagedList_ShouldReturnEmptyPagedResult_WhenSourceIsEmpty()
    {
        // Arrange
        var source = new List<string>(); // List rỗng để kích hoạt nhánh if (count == 0)

        // Act
        var result = source.ToPagedList(1, 10);

        // Assert
        result.Should().BeOfType<PagedResult<string>>();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public void ToPagedList_ShouldReturnFirstPage_WhenSourceHasData()
    {
        // Arrange
        var source = new List<string> { "Cà phê đen", "Bạc xỉu", "Trà đào", "Trà vải", "Cacao" };

        // Act
        var result = source.ToPagedList(1, 2); // Lấy trang 1, mỗi trang 2 ly

        // Assert
        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(2);
        result.Items.Should().ContainInOrder("Cà phê đen", "Bạc xỉu");
        result.Page.Should().Be(1);
    }

    [Fact]
    public void ToPagedList_ShouldReturnSecondPage_WhenSourceHasData()
    {
        // Arrange
        var source = new List<string> { "Cà phê đen", "Bạc xỉu", "Trà đào", "Trà vải", "Cacao" };

        // Act
        var result = source.ToPagedList(2, 2); // Lấy trang 2, mỗi trang 2 ly

        // Assert
        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(2);
        // Nhánh toán học Skip/Take hoạt động đúng thì phải lấy ra ly số 3 và 4
        result.Items.Should().ContainInOrder("Trà đào", "Trà vải");
        result.Page.Should().Be(2);
    }

    [Fact]
    public void ToPagedList_ShouldReturnRemainingItems_WhenOnLastPageWithOddNumber()
    {
        // Arrange
        var source = new List<string> { "Cà phê đen", "Bạc xỉu", "Trà đào", "Trà vải", "Cacao" };

        // Act
        var result = source.ToPagedList(3, 2); // Lấy trang 3, mỗi trang 2 ly -> Chỉ còn đúng 1 ly Cacao

        // Assert
        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(1);
        result.Items.Should().Contain("Cacao");
        result.Page.Should().Be(3);
    }

    [Fact]
    public void ToPagedList_ShouldHandleIEnumerableCorrectly_WhenSourceIsNotList()
    {
        // Arrange
        // Dùng LINQ Select để ép nó thành IEnumerable thuần túy
        // Mục đích: kích hoạt đoạn fallback `?? source.ToList()` trong code gốc của bro
        var source = new[] { 1, 2, 3 }.Select(x => $"Món số {x}");

        // Act
        var result = source.ToPagedList(1, 10);

        // Assert
        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(3);
    }
}