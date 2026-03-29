namespace WarehouseService.Application.Tests.Extensions;

using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using WarehouseService.Application.Features.StockTransactions.Command.CreateStockIn;
using WarehouseService.Application.Extensions;
using WarehouseService.Application.Behaviors;

public class ApplicationServiceRegistrationTests
{
    [Fact]
    public void AddApplication_ShouldRegisterCoreServicesToServiceCollection()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        services.Should().Contain(d => d.ServiceType == typeof(IMediator));

        // Kiểm tra ValidationBehavior đã được đăng ký vào Pipeline chưa
        services.Should().Contain(d =>
            d.ServiceType == typeof(IPipelineBehavior<,>) &&
            d.ImplementationType == typeof(ValidationBehavior<,>));
    }

    [Fact]
    public void AddApplication_ShouldRegisterValidatorsFromApplicationAssembly()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        // Kiểm tra xem có bất kỳ IValidator nào được đăng ký không
        var validatorDescriptors = services.Where(d =>
            d.ServiceType.IsGenericType &&
            d.ServiceType.GetGenericTypeDefinition() == typeof(IValidator<>))
            .ToList();

        validatorDescriptors.Should().NotBeEmpty();
    }

    [Fact]
    public void AddApplication_ShouldBeAbleToResolveMediatorWithBehaviors()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddApplication();

        // Mock các dependencies cần thiết cho Handler thật nếu có (hoặc dùng Handler đơn giản nhất)
        // Ở đây ta chỉ test khả năng Resolve từ DI Container
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var mediator = serviceProvider.GetService<IMediator>();

        // Sử dụng CreateStockInCommand - Một Command "xịn" trong project Application
        var validator = serviceProvider.GetService<IValidator<CreateStockInCommand>>();

        // Assert
        mediator.Should().NotBeNull();
        validator.Should().NotBeNull("vì CreateStockInCommandValidator nằm trong assembly Application đã được scan");
    }
}