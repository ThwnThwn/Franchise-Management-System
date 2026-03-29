using SalesService.Application.Helpers;
using FluentAssertions;
using Xunit;
using System.Collections.Generic;

namespace SalesService.Application.Tests.Helpers;

public class VNPayHelperTests
{
    private const string HashSecret = "SECRET";

    [Fact]
    public void BuildPaymentUrl_Should_ReturnCorrectUrl()
    {
        var parameters = new Dictionary<string, string>
        {
            ["vnp_Amount"] = "100000",
            ["vnp_TxnRef"] = "REF123"
        };

        var url = VNPayHelper.BuildPaymentUrl("https://example.com/pay", HashSecret, parameters);

        url.Should().Contain("vnp_Amount=100000");
        url.Should().Contain("vnp_TxnRef=REF123");
        url.Should().Contain("vnp_SecureHash=");
    }

    [Fact]
    public void VerifySignature_Should_ReturnTrue_When_HashMatches()
    {
        var parameters = new Dictionary<string, string>
        {
            ["vnp_Amount"] = "100000",
            ["vnp_TxnRef"] = "REF123"
        };
        
        // Manual hash calculation for expectation
        var query = "vnp_Amount=100000&vnp_TxnRef=REF123";
        var expectedHash = VNPayHelper.HmacSHA512(HashSecret, query);

        var isValid = VNPayHelper.VerifySignature(HashSecret, parameters, expectedHash);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateSignature_Should_ReturnTrue_When_HashMatches()
    {
        var query = new Dictionary<string, string>
        {
            ["vnp_Amount"] = "100000",
            ["vnp_TxnRef"] = "REF123"
        };
        
        var rawData = "vnp_Amount=100000&vnp_TxnRef=REF123";
        var hash = VNPayHelper.HmacSHA512(HashSecret, rawData);
        query["vnp_SecureHash"] = hash;

        var isValid = VNPayHelper.ValidateSignature(query, HashSecret);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateSignature_Should_ReturnFalse_When_HashMissing()
    {
        var query = new Dictionary<string, string> { ["vnp_Amount"] = "100" };
        var isValid = VNPayHelper.ValidateSignature(query, HashSecret);
        isValid.Should().BeFalse();
    }
}
