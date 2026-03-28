using EngagementService.Application.Behaviors;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;

namespace EngagementService.Application.Tests.Behaviors;

public class ValidationBehaviorTests
{
    // Tạo class giả lập cho Request và Response để test
    public record TestRequest : IRequest<string>;

    // 1. Phủ nhánh: Không có Validator nào (Dòng 23-24)
    [Fact]
    public async Task Handle_ShouldCallNext_WhenNoValidatorsExist()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var request = new TestRequest();
        var nextCalled = false;

        // Act
        await behavior.Handle(request, CancellationToken.None, () =>
        {
            nextCalled = true;
            return Task.FromResult("Success");
        });

        // Assert
        nextCalled.Should().BeTrue();
    }

    // 2. Phủ xanh Happy Path: Validation thành công (Dòng 48)
    [Fact]
    public async Task Handle_ShouldCallNext_WhenValidationSucceeds()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<TestRequest>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult()); // Trả về kết quả không lỗi

        var behavior = new ValidationBehavior<TestRequest, string>(new[] { validatorMock.Object });
        var request = new TestRequest();
        var nextCalled = false;

        // Act
        await behavior.Handle(request, CancellationToken.None, () =>
        {
            nextCalled = true;
            return Task.FromResult("Success");
        });

        // Assert
        nextCalled.Should().BeTrue();
        validatorMock.Verify(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // 3. Phủ nhánh: Validation thất bại -> Throw Exception (Dòng 41-46)
    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenFailuresExist()
    {
        // Arrange
        var failures = new List<ValidationFailure> { new("PropertyName", "Error Message") };
        var validatorMock = new Mock<IValidator<TestRequest>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult(failures));

        var behavior = new ValidationBehavior<TestRequest, string>(new[] { validatorMock.Object });
        var request = new TestRequest();

        // Act
        var act = () => behavior.Handle(request, CancellationToken.None, () => Task.FromResult("Success"));

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
                 .Where(e => e.Errors.Count() == 1);
    }
}