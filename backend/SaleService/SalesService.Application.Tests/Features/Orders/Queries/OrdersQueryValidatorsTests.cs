using FluentAssertions;
using SalesService.Application.Features.Orders.Queries.GetById;
using SalesService.Application.Features.Orders.Queries.GetOrders;
using SharedKernel.Abstractions;

namespace SalesService.Application.Tests.Features.Orders.Queries;

public class OrdersQueryValidatorsTests
{
    public class GetOrderByIdQueryValidatorTests
    {
        private readonly GetOrderByIdQueryValidator _validator = new();

        [Fact]
        public void Validate_ShouldHaveError_WhenOrderIdIsEmpty()
        {
            var query = new GetOrderByIdQuery(Guid.Empty);
            var result = _validator.Validate(query);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "OrderId is required.");
        }

        [Fact]
        public void Validate_ShouldNotHaveError_WhenValid()
        {
            var query = new GetOrderByIdQuery(Guid.NewGuid());
            var result = _validator.Validate(query);
            result.IsValid.Should().BeTrue();
        }
    }

    public class GetOrdersQueryValidatorTests
    {
        private readonly GetOrdersQueryValidator _validator = new();

        [Fact]
        public void Validate_ShouldHaveError_WhenFromDateGreaterThanToday()
        {
            var query = new GetOrdersQuery { FromDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)) };
            var result = _validator.Validate(query);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Ngày bắt đầu lọc không thể lớn hơn ngày hiện tại.");
        }

        [Fact]
        public void Validate_ShouldHaveError_WhenFromDateGreaterThanToDate()
        {
            var query = new GetOrdersQuery 
            { 
                FromDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
                ToDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-2))
            };
            var result = _validator.Validate(query);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Ngày bắt đầu lọc không thể lớn hơn ngày kết thúc lọc.");
        }

        [Fact]
        public void Validate_ShouldNotHaveError_WhenValid()
        {
            var query = new GetOrdersQuery 
            { 
                FromDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-2)),
                ToDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1))
            };
            var result = _validator.Validate(query);
            result.IsValid.Should().BeTrue();
        }
    }
}
