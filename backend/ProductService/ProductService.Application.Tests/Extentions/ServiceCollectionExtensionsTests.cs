using AutoMapper;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.Behaviors;
using ProductService.Application.Extentions;

namespace ProductService.Application.Tests.Extentions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddApplication_ShouldRegisterAllRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplication();

        // Assert
        // 1. Kiểm tra AutoMapper đã được đăng ký dạng Singleton chưa
        var mapperDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IMapper));
        mapperDescriptor.Should().NotBeNull("AutoMapper phải được đăng ký vào DI container.");
        mapperDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton, "AutoMapper bắt buộc phải là Singleton để tối ưu hiệu năng.");

        // 2. Kiểm tra MediatR đã vào rổ chưa
        var mediatorDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IMediator));
        mediatorDescriptor.Should().NotBeNull("MediatR phải được đăng ký để xài CQRS.");

        // 3. Kiểm tra Pipeline Behavior (ValidationBehavior) đã được móc vào MediatR chưa
        var pipelineDescriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IPipelineBehavior<,>) &&
            d.ImplementationType == typeof(ValidationBehavior<,>));

        pipelineDescriptor.Should().NotBeNull("ValidationBehavior phải được móc vào Pipeline của MediatR.");
        pipelineDescriptor!.Lifetime.Should().Be(ServiceLifetime.Transient, "Pipeline behavior nên là Transient.");

        // 4. Kiểm tra FluentValidation đã quét được Validator nào trong Application Assembly chưa
        // (Do mấy file trước anh em mình đã tạo GetProductByIdValidator, UpdateProductCommandValidator... nên chắc chắn list này phải > 0)
        var hasValidators = services.Any(d =>
            d.ServiceType.IsGenericType &&
            d.ServiceType.GetGenericTypeDefinition() == typeof(IValidator<>));

        hasValidators.Should().BeTrue("FluentValidation phải tìm thấy và đăng ký ít nhất 1 class Validator.");

        // 5. TEST THỰC CHIẾN: Thử Build ra ServiceProvider và lấy IMapper ra xem có bị nổ exception không
        // Bước này cực kỳ quan trọng để đảm bảo logic c.AddMaps() bên trong cấu hình không bị crash
        var serviceProvider = services.BuildServiceProvider();
        var mapper = serviceProvider.GetService<IMapper>();
        mapper.Should().NotBeNull("Phải resolve được IMapper ra xài thực tế.");
    }
}