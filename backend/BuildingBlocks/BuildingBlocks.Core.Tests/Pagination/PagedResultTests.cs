using BuildingBlocks.Core.Pagination;
using FluentAssertions;

namespace BuildingBlocks.Core.Tests.Pagination;

public class PagedResultTests
{
    [Theory]
    [InlineData(10, 5, 2)] // 10 ly, mỗi trang 5 ly -> chẵn 2 trang
    [InlineData(11, 5, 3)] // 11 ly, mỗi trang 5 ly -> lẻ 1 ly -> 3 trang
    [InlineData(0, 10, 0)] // Không có ly nào -> 0 trang
    public void Constructor_ShouldCalculateTotalPagesCorrectly(long totalCount, int pageSize, int expectedTotalPages)
    {
        // Arrange
        var items = new List<string>(); // Dữ liệu ảo không quan trọng trong test toán học này

        // Act
        var result = new PagedResult<string>(items, totalCount, 1, pageSize);

        // Assert
        result.TotalPages.Should().Be(expectedTotalPages);
    }

    [Theory]
    [InlineData(1, 3, false, true)]  // Đang ở trang 1, tổng 3 trang -> Không có Prev, CÓ Next
    [InlineData(2, 3, true, true)]   // Đang ở trang 2, tổng 3 trang -> CÓ Prev, CÓ Next
    [InlineData(3, 3, true, false)]  // Đang ở trang 3 (cuối), tổng 3 trang -> CÓ Prev, Không có Next
    [InlineData(1, 1, false, false)] // Chỉ có đúng 1 trang -> Không Prev, Không Next
    public void NavigationFlags_ShouldBeSetCorrectly_BasedOnCurrentPage(
        int currentPage, int totalPages, bool expectedHasPrev, bool expectedHasNext)
    {
        // Arrange
        // Tính ngược lại totalCount để tạo ra đúng số totalPages mong muốn (Giả sử pageSize = 10)
        long totalCount = totalPages * 10;
        var items = new List<string> { "Cà phê đen" }; // Dummy data

        // Act
        var result = new PagedResult<string>(items, totalCount, currentPage, 10);

        // Assert
        result.HasPreviousPage.Should().Be(expectedHasPrev);
        result.HasNextPage.Should().Be(expectedHasNext);
    }

    [Fact]
    public void Create_FactoryMethod_ShouldReturnValidInstance()
    {
        // Arrange
        var items = new List<string> { "Bạc xỉu", "Cà phê sữa" };

        // Act
        var result = PagedResult<string>.Create(items, 20, 2, 5);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEquivalentTo(items);
        result.TotalCount.Should().Be(20);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalPages.Should().Be(4); // 20 / 5 = 4
    }
}