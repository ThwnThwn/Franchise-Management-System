using Ardalis.Result;
using AutoMapper;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using ProductService.Application.Features.Categories.Queries.GetById;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;
using static ProductService.Application.Features.Categories.Queries.GetById.GetCategoryByIdQuery;

namespace ProductService.Application.Tests.Features.Categories;

public class GetByIdHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetCategoryByIdQueryHandler _handler;
    private readonly GetCategoryByIdValidator _validator;

    public GetByIdHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetCategoryByIdQueryHandler(_categoryRepositoryMock.Object, _mapperMock.Object);
        _validator = new GetCategoryByIdValidator();
    }

    #region Handler Tests

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new GetCategoryByIdQuery(categoryId);

        _categoryRepositoryMock
            .Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().Contain($"Category with id '{categoryId}' not found.");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCategoryIsFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new GetCategoryByIdQuery(categoryId);

        var categoryEntity = new Category { Id = categoryId, Name = "Existing Category" };
        var expectedResponse = new GetCategoryByIdResponse
        {
            Id = categoryEntity.Id,
            Name = categoryEntity.Name
        };

        _categoryRepositoryMock
            .Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync(categoryEntity);

        _mapperMock
            .Setup(m => m.Map<GetCategoryByIdResponse>(categoryEntity))
            .Returns(expectedResponse);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(categoryId);
        result.Value.Name.Should().Be("Existing Category");

        // Đảm bảo repo được gọi đúng 1 lần với ID chính xác
        _categoryRepositoryMock.Verify(repo => repo.GetByIdAsync(categoryId), Times.Once);
    }

    #endregion Handler Tests

    #region Validator Tests

    [Fact]
    public void Validator_ShouldHaveError_WhenCategoryIdIsEmpty()
    {
        // Arrange
        var query = new GetCategoryByIdQuery(Guid.Empty);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        // FluentValidation sẽ trigger cả 2 lỗi (NotEmpty và Must) khi truyền Guid.Empty
        result.ShouldHaveValidationErrorFor(x => x.CategoryId)
              .WithErrorMessage("Category ID is required");

        result.ShouldHaveValidationErrorFor(x => x.CategoryId)
              .WithErrorMessage("Category Id must be a valid non-empty GUID.");
    }

    [Fact]
    public void Validator_ShouldNotHaveError_WhenCategoryIdIsValid()
    {
        // Arrange
        var query = new GetCategoryByIdQuery(Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CategoryId);
    }

    #endregion Validator Tests
}