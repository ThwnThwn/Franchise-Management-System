using BuildingBlocks.Core.Wrapper;
using FluentAssertions;

namespace BuildingBlocks.Core.Tests.Wrapper;

public class ResultTests
{
    // 1. Test cho class Result (Void)
    [Fact]
    public void Success_VoidResult_ShouldReturnIsSuccessTrue()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    // 2. Test cho class Result<T> (Có Data) sử dụng Factory Method
    [Fact]
    public void Success_GenericResult_ShouldReturnDataAndIsSuccessTrue()
    {
        // Arrange
        // Giả lập một ly nước xịn xò để bọc vào Result
        var drinkData = "Cà phê sữa đá đặc ruột";

        // Act
        var result = Result<string>.Success(drinkData);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(drinkData);
    }

    // 3. Test CỰC KỲ QUAN TRỌNG: Ép kiểu ngầm định (Implicit Operator)
    [Fact]
    public void ImplicitOperator_ShouldConvertDataToResultCorrectly()
    {
        // Arrange
        var drinkData = "Bạc xỉu nhiều sữa";

        // Act
        // Gán trực tiếp string vào Result<string> để kích hoạt hàm operator implicit
        Result<string> result = drinkData;

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(drinkData);
    }

    // 4. Test với Data là một Object phức tạp (đúng chuẩn DTO trả về)
    [Fact]
    public void Success_WithComplexObject_ShouldWrapDataCorrectly()
    {
        // Arrange
        var drinkObj = new { Id = Guid.NewGuid(), Name = "Trà đào cam sả", Price = 45000 };

        // Act
        var result = Result<object>.Success(drinkObj);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(drinkObj);
    }
}