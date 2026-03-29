using System.Web;
using FluentAssertions;
using SupplierService.Application.Helpers;

namespace SupplierService.Application.Tests.Helpers;

public class VNPayHelperTests
{
    private const string TestHashSecret = "CFDENKHONGDUONG1234567890SECRETKEY";
    private const string TestBaseUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

    #region HmacSHA512 Tests

    [Fact]
    public void HmacSHA512_ShouldReturnConsistent128CharHexString()
    {
        // Arrange
        var key = "MySecretKey";
        var inputData = "vnp_Amount=3500000&vnp_OrderInfo=Thanh toan ca phe";

        // Act
        var hash1 = VNPayHelper.HmacSHA512(key, inputData);
        var hash2 = VNPayHelper.HmacSHA512(key, inputData);

        // Assert
        // SHA512 băm ra chuỗi hex luôn luôn dài đúng 128 ký tự
        hash1.Should().HaveLength(128);
        // Băm cùng 1 data và key thì kết quả phải y hệt nhau
        hash1.Should().Be(hash2);
    }

    #endregion HmacSHA512 Tests

    #region BuildPaymentUrl Tests

    [Fact]
    public void BuildPaymentUrl_ShouldFilterEmptyParams_AndSortKeys_AndUrlEncode()
    {
        // Arrange
        // Cố tình xếp lộn xộn, thêm dấu cách, thêm giá trị rỗng/null để test
        var parameters = new Dictionary<string, string>
        {
            { "vnp_Version", "2.1.0" },
            { "vnp_OrderInfo", "Thanh toan 2 ly bac xiu" }, // Có khoảng trắng để test UrlEncode
            { "vnp_Amount", "7000000" }, // 70k * 100
            { "vnp_BankCode", "" }, // Value rỗng -> Cần bị loại bỏ
            { "vnp_IpAddr", null! } // Value null -> Cần bị loại bỏ
        };

        // Act
        var paymentUrl = VNPayHelper.BuildPaymentUrl(TestBaseUrl, TestHashSecret, parameters);

        // Assert
        paymentUrl.Should().NotBeNullOrWhiteSpace();
        paymentUrl.Should().StartWith(TestBaseUrl);

        // Kiểm tra xem các param rỗng/null đã bị lọc bỏ chưa
        paymentUrl.Should().NotContain("vnp_BankCode");
        paymentUrl.Should().NotContain("vnp_IpAddr");

        // Kiểm tra xem đã sắp xếp chuẩn A-Z chưa (Amount phải đứng trước OrderInfo và Version)
        // và khoảng trắng có được encode thành dấu "+" (do xài WebUtility.UrlEncode) không
        var expectedQueryString = "vnp_Amount=7000000&vnp_OrderInfo=Thanh+toan+2+ly+bac+xiu&vnp_Version=2.1.0";
        paymentUrl.Should().Contain(expectedQueryString);

        // Kiểm tra đuôi bảo mật
        paymentUrl.Should().Contain("&vnp_SecureHashType=HmacSHA512&vnp_SecureHash=");
    }

    #endregion BuildPaymentUrl Tests

    #region VerifySignature Tests

    [Fact]
    public void VerifySignature_ShouldReturnTrue_WhenSignatureIsCorrect()
    {
        // Arrange
        var parameters = new Dictionary<string, string>
        {
            { "vnp_Amount", "3500000" },
            { "vnp_OrderInfo", "Thanh toan tra dao cam sa" },
            { "vnp_TransactionNo", "12345678" }
        };

        // Bước 1: Giả lập quá trình VNPay tạo Hash (dùng chính hàm của mình để ra đúng kết quả)
        var sortedParams = new SortedDictionary<string, string>(parameters, StringComparer.Ordinal);
        var query = string.Join("&", sortedParams.Select(kv => $"{HttpUtility.UrlEncode(kv.Key)}={HttpUtility.UrlEncode(kv.Value)}"));
        var validSecureHash = VNPayHelper.HmacSHA512(TestHashSecret, query);

        // Act
        var isValid = VNPayHelper.VerifySignature(TestHashSecret, parameters, validSecureHash);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifySignature_ShouldReturnTrue_EvenWhenSignatureIsUppercase()
    {
        // Arrange
        var parameters = new Dictionary<string, string>
        {
            { "vnp_Amount", "1500000" }, // Ly đen đá 15k
            { "vnp_OrderInfo", "Don hang 001" }
        };

        var sortedParams = new SortedDictionary<string, string>(parameters, StringComparer.Ordinal);
        var query = string.Join("&", sortedParams.Select(kv => $"{HttpUtility.UrlEncode(kv.Key)}={HttpUtility.UrlEncode(kv.Value)}"));

        // Cố tình ToUpper() cái mã Hash để test tính năng StringComparison.OrdinalIgnoreCase
        var uppercaseSecureHash = VNPayHelper.HmacSHA512(TestHashSecret, query).ToUpper();

        // Act
        var isValid = VNPayHelper.VerifySignature(TestHashSecret, parameters, uppercaseSecureHash);

        // Assert
        isValid.Should().BeTrue("Vì hàm Verify dùng OrdinalIgnoreCase nên chữ HOA chữ thường đều phải pass.");
    }

    [Fact]
    public void VerifySignature_ShouldReturnFalse_WhenSignatureIsIncorrect()
    {
        // Arrange
        var parameters = new Dictionary<string, string>
        {
            { "vnp_Amount", "5000000" },
            { "vnp_OrderInfo", "Thanh toan fake" }
        };

        var fakeSecureHash = "1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef"; // Hash lụi

        // Act
        var isValid = VNPayHelper.VerifySignature(TestHashSecret, parameters, fakeSecureHash);

        // Assert
        isValid.Should().BeFalse();
    }

    #endregion VerifySignature Tests
}