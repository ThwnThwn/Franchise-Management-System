using BuildingBlocks.Core.Pagination;
using FluentAssertions;

namespace BuildingBlocks.Core.Tests.Pagination;

public class PaginationRequestTests
{
    // Tạo một class dummy kế thừa từ abstract class để test logic
    private class TestPaginationRequest : PaginationRequest
    {
    }

    [Fact]
    public void Constructor_ShouldHaveCorrectDefaultValues()
    {
        // Act
        var request = new TestPaginationRequest();

        // Assert
        request.Page.Should().Be(1);
        request.PageSize.Should().Be(10);
    }

    [Theory]
    [InlineData(5, 5)]   // Set hợp lệ
    [InlineData(0, 1)]   // Dưới 1 -> Trả về 1
    [InlineData(-5, 1)]  // Số âm -> Trả về 1
    public void Page_Setter_ShouldClampValuesCorrectly(int inputValue, int expectedValue)
    {
        // Act
        var request = new TestPaginationRequest { Page = inputValue };

        // Assert
        request.Page.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(20, 20)]  // Set hợp lệ (nằm trong khoảng 1 - 50)
    [InlineData(0, 10)]   // Dưới 1 -> Fallback về default là 10
    [InlineData(-15, 10)] // Số âm -> Fallback về default là 10
    [InlineData(51, 50)]  // Vượt trần MaxPageSize (50) -> Ép về 50
    [InlineData(100, 50)] // Vượt xa trần -> Ép về 50
    public void PageSize_Setter_ShouldClampValuesCorrectly(int inputValue, int expectedValue)
    {
        // Act
        var request = new TestPaginationRequest { PageSize = inputValue };

        // Assert
        request.PageSize.Should().Be(expectedValue);
    }
}