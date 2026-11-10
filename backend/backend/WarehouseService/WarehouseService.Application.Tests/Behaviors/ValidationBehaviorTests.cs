using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using WarehouseService.Application.Behaviors;

namespace WarehouseService.Application.Tests.Behaviors;

public class ValidationBehaviorTests
{
    // 1. Tạo Dummy Request & Response mang đậm mùi F&B để test Behavior
    public class OrderCoffeeCommand : IRequest<OrderCoffeeResponse>
    {
        public string DrinkName { get; set; } = string.Empty;
    }

    public class OrderCoffeeResponse
    {
        public Guid OrderId { get; set; }
    }

    [Fact]
    public async Task Handle_ShouldInvokeNext_WhenNoValidatorsExist()
    {
        // Arrange
        // Kịch bản: Request không có validator nào được đăng ký
        var validators = Enumerable.Empty<IValidator<OrderCoffeeCommand>>();
        var behavior = new ValidationBehavior<OrderCoffeeCommand, OrderCoffeeResponse>(validators);
        var request = new OrderCoffeeCommand { DrinkName = "Cà phê sữa đá" };

        var nextDelegateMock = new Mock<RequestHandlerDelegate<OrderCoffeeResponse>>();
        nextDelegateMock.Setup(next => next()).ReturnsAsync(new OrderCoffeeResponse { OrderId = Guid.NewGuid() });

        // Act
        var result = await behavior.Handle(request, nextDelegateMock.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        // Đảm bảo pipeline được chạy tiếp
        nextDelegateMock.Verify(next => next(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldInvokeNext_WhenValidationPasses()
    {
        // Arrange
        // Kịch bản: Validator chạy và không phát hiện lỗi
        var validatorMock = new Mock<IValidator<OrderCoffeeCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<OrderCoffeeCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult()); // Trả về list lỗi rỗng

        var validators = new List<IValidator<OrderCoffeeCommand>> { validatorMock.Object };
        var behavior = new ValidationBehavior<OrderCoffeeCommand, OrderCoffeeResponse>(validators);
        var request = new OrderCoffeeCommand { DrinkName = "Bạc xỉu" };

        var nextDelegateMock = new Mock<RequestHandlerDelegate<OrderCoffeeResponse>>();
        nextDelegateMock.Setup(next => next()).ReturnsAsync(new OrderCoffeeResponse { OrderId = Guid.NewGuid() });

        // Act
        var result = await behavior.Handle(request, nextDelegateMock.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        // Đảm bảo pipeline được chạy tiếp
        nextDelegateMock.Verify(next => next(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidationFails()
    {
        // Arrange
        // Kịch bản: Validator phát hiện lỗi "Tên món nước không được để trống"
        var validationFailures = new List<ValidationFailure>
        {
            new("DrinkName", "Tên món nước không được để trống.")
        };

        var validatorMock = new Mock<IValidator<OrderCoffeeCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<OrderCoffeeCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures)); // CÓ lỗi

        var validators = new List<IValidator<OrderCoffeeCommand>> { validatorMock.Object };
        var behavior = new ValidationBehavior<OrderCoffeeCommand, OrderCoffeeResponse>(validators);
        var request = new OrderCoffeeCommand { DrinkName = "" };

        var nextDelegateMock = new Mock<RequestHandlerDelegate<OrderCoffeeResponse>>();

        // Act
        // Gói action lại để dùng FluentAssertions kiểm tra văng Exception
        var action = async () => await behavior.Handle(request, nextDelegateMock.Object, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.Count() == 1 && ex.Errors.First().ErrorMessage == "Tên món nước không được để trống.");

        // CỰC KỲ QUAN TRỌNG: Đảm bảo pipeline BỊ CHẶN LẠI, hàm next() KHÔNG BAO GIỜ được gọi
        nextDelegateMock.Verify(next => next(), Times.Never);
    }
}