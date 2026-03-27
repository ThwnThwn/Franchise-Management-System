using Ardalis.Result;
using AutoMapper;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using ProductService.Application.Features.Categories.Queries.GetByCategoryName;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;
using static ProductService.Application.Features.Categories.Queries.GetByCategoryName.GetCategoryByNameQuery;

namespace ProductService.Application.Tests.Features.Categories;

public class GetByCategoryNameHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetCategoryByNameQueryHandler _handler;
    private readonly GetCategoryByNameQueryValidator _validator;

    public GetByCategoryNameHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetCategoryByNameQueryHandler(_categoryRepositoryMock.Object, _mapperMock.Object);
        _validator = new GetCategoryByNameQueryValidator();
    }

    #region Handler Tests

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Handle_ShouldReturnInvalid_WhenCategoryNameIsNullOrWhiteSpace(string? invalidName)
    {
        // Arrange
        // (Dùng "!" để bypass warning nullable khi test case null)
        var query = new GetCategoryByNameQuery(invalidName!);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().ContainSingle(e => e.ErrorMessage == "Category name cannot be empty.");

        // Đảm bảo không gọi xuống DB
        _categoryRepositoryMock.Verify(repo => repo.GetByCategoryNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        var categoryName = "NonExistentCategory";
        var query = new GetCategoryByNameQuery(categoryName);

        _categoryRepositoryMock
            .Setup(repo => repo.GetByCategoryNameAsync(categoryName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().Contain($"Category with name '{categoryName}' not found.");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCategoryIsFound()
    {
        // Arrange
        var categoryName = "ExistingCategory";
        var query = new GetCategoryByNameQuery(categoryName);

        var categoryEntity = new Category { Id = Guid.NewGuid(), Name = categoryName };
        var expectedResponse = new GetCategoryByNameQueryResponse
        {
            Id = categoryEntity.Id,
            Name = categoryEntity.Name
        };

        _categoryRepositoryMock
            .Setup(repo => repo.GetByCategoryNameAsync(categoryName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoryEntity);

        _mapperMock
            .Setup(m => m.Map<GetCategoryByNameQueryResponse>(categoryEntity))
            .Returns(expectedResponse);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(categoryEntity.Id);
        result.Value.Name.Should().Be(categoryName);

        // Đảm bảo repo được gọi đúng 1 lần với tên chính xác
        _categoryRepositoryMock.Verify(repo => repo.GetByCategoryNameAsync(categoryName, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion Handler Tests

    #region Validator Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validator_ShouldHaveError_WhenCategoryNameIsEmpty(string? invalidName)
    {
        // Arrange
        var query = new GetCategoryByNameQuery(invalidName!);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryName)
              .WithErrorMessage("Category name is required");
    }

    [Fact]
    public void Validator_ShouldNotHaveError_WhenCategoryNameIsValid()
    {
        // Arrange
        var query = new GetCategoryByNameQuery("Valid Category Name");

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CategoryName);
    }

    #endregion Validator Tests
}