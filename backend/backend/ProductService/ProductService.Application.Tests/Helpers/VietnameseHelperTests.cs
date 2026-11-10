using FluentAssertions;
using ProductService.Application.Helpers;

namespace ProductService.Application.Tests.Helpers;

public class VietnameseHelperTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RemoveVietnameseSigns_ShouldReturnOriginalString_WhenInputIsNullOrWhiteSpace(string? input)
    {
        // Act
        // Dùng null! để bypass cảnh báo của trình biên dịch lúc truyền null
        var result = VietnameseHelper.RemoveVietnameseSigns(input!);

        // Assert
        result.Should().Be(input);
    }

    [Theory]
    // 1. Kịch bản chuỗi không có dấu sẵn -> Trả về y nguyên
    [InlineData("Ca phe den da", "Ca phe den da")]
    [InlineData("Espresso 100%", "Espresso 100%")]

    // 2. Kịch bản chuẩn domain F&B (Chữ thường, hoa, có dấu)
    [InlineData("Cà phê sữa đá", "Ca phe sua da")]
    [InlineData("BẠC XỈU NÓNG", "BAC XIU NONG")]
    [InlineData("Trà Đào Cam Sả 50% Đường!", "Tra Dao Cam Sa 50% Duong!")]

    // 3. Kịch bản test riêng chữ Đ, đ (Chữ này hay bị lọt lưới nhất trong tiếng Việt)
    [InlineData("Đường đen đập đá", "Duong den dap da")]
    [InlineData("ĐĐ đđ", "DD dd")]

    // 4. Kịch bản test stress-test toàn bộ các nguyên âm
    [InlineData("ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ", "AAAAAAAAAAAAAAAAA")] // Test họ nhà Á
    [InlineData("éèẹẻẽêếềệểễ", "eeeeeeeeeee")] // Test họ nhà é
    [InlineData("óòọỏõôốồộổỗơớờợởỡ", "ooooooooooooooooo")] // Test họ nhà ó
    [InlineData("úùụủũưứừựửữ", "uuuuuuuuuuu")] // Test họ nhà ú
    [InlineData("íìịỉĩ", "iiiii")] // Test họ nhà í
    [InlineData("ýỳỵỷỹ", "yyyyy")] // Test họ nhà ý
    public void RemoveVietnameseSigns_ShouldRemoveAccentsCorrectly_WhenInputHasVietnameseSigns(string input, string expected)
    {
        // Act
        var result = VietnameseHelper.RemoveVietnameseSigns(input);

        // Assert
        result.Should().Be(expected);
    }
}