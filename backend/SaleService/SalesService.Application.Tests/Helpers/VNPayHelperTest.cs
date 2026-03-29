using FluentAssertions;
using SalesService.Application.Helpers;
using Xunit;
using System.Collections.Generic;

namespace SalesService.Application.Tests.Helpers;

public class VNPayHelperTest
{
    [Fact]
    public void BuildPaymentUrl_Should_ReturnUrl_With_Signature()
    {
        var parameters = new Dictionary<string, string>
        {
            { "vnp_Amount", "1000000" },
            { "vnp_OrderInfo", "test" }
        };
        var secret = "secret";
        var baseUrl = "https://vnpay.vn";

        var url = VNPayHelper.BuildPaymentUrl(baseUrl, secret, parameters);

        url.Should().Contain("vnp_SecureHash=");
        url.Should().Contain("vnp_Amount=1000000");
    }

    [Fact]
    public void VerifySignature_Should_ReturnTrue_When_Valid()
    {
        var secret = "secret";
        var parameters = new Dictionary<string, string> { { "a", "1" }, { "b", "2" } };
        // a=1&b=2
        var hash = VNPayHelper.HmacSHA512(secret, "a=1&b=2");

        var isValid = VNPayHelper.VerifySignature(secret, parameters, hash);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateSignature_Should_ReturnTrue_When_Valid()
    {
        var secret = "secret";
        var query = new Dictionary<string, string> { { "a", "1" }, { "b", "2" } };
        var hash = VNPayHelper.HmacSHA512(secret, "a=1&b=2");
        query["vnp_SecureHash"] = hash;

        var isValid = VNPayHelper.ValidateSignature(query, secret);

        isValid.Should().BeTrue();
    }
}
