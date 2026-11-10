using Ardalis.Result;
using AutoMapper;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using ProductService.Application.Features.Categories.Command.CreateCategory;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;
using static ProductService.Application.Features.Categories.Command.CreateCategory.CreateCategoryCommand;

namespace ProductService.Application.Tests.Features.Categories;

public class CreateCategoryHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CreateCategoryCommandHandler _handler;
    private readonly CreateCategoryCommandValidator _validator;

    public CreateCategoryHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new CreateCategoryCommandHandler(_categoryRepositoryMock.Object, _mapperMock.Object);
        _validator = new CreateCategoryCommandValidator();
    }

    #region Handler Tests

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenCategoryNameAlreadyExists()
    {
        // Arrange
        var request = new CreateCategoryCommandRequest { Name = "Existing Category" };
        var command = new CreateCategoryCommand(request);

        _categoryRepositoryMock
            .Setup(repo => repo.ExistsByNameAsync(request.Name,
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.Conflict);
        result.Errors.Should().Contain($"Category with name '{request.Name}' already exists.");

        // Ensure AddAsync was never called
        _categoryRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCategoryIsCreatedSuccessfully()
    {
        // Arrange
        var request = new CreateCategoryCommandRequest { Name = "New Category", Description = "Description" };
        var command = new CreateCategoryCommand(request);
        var categoryEntity = new Category { Name = request.Name, Description = request.Description };
        var expectedResponse = new CreateCategoryCommandResponse { Id = Guid.NewGuid(), Name = request.Name, Description = request.Description };

        _categoryRepositoryMock
            .Setup(repo => repo.ExistsByNameAsync(request.Name,
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mapperMock.Setup(m => m.Map<Category>(request)).Returns(categoryEntity);
        _mapperMock.Setup(m => m.Map<CreateCategoryCommandResponse>(It.IsAny<Category>())).Returns(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(request.Name);

        // Ensure ID was generated before saving
        categoryEntity.Id.Should().NotBeEmpty();

        // Verify Repository interaction
        _categoryRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Category>(c => c.Name == request.Name)), Times.Once);
    }

    #endregion Handler Tests

    #region Validator Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validator_ShouldHaveError_WhenNameIsEmpty(string invalidName)
    {
        // Arrange
        var command = new CreateCategoryCommand(new CreateCategoryCommandRequest { Name = invalidName });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Name);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenNameExceedsMaxLength()
    {
        // Arrange
        var longName = new string('a', 256);
        var command = new CreateCategoryCommand(new CreateCategoryCommandRequest { Name = longName });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Name)
              .WithErrorMessage("Category name must not exceed 255 characters");
    }

    [Theory]
    [InlineData("Invalid@Name")]
    [InlineData("Category!")]
    [InlineData("Name with #hashtag")]
    public void Validator_ShouldHaveError_WhenNameContainsInvalidCharacters(string invalidName)
    {
        // Arrange
        var command = new CreateCategoryCommand(new CreateCategoryCommandRequest { Name = invalidName });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Name)
              .WithErrorMessage("Category name contains invalid characters");
    }

    [Theory]
    [InlineData("ValidName123")]
    [InlineData("Valid-Name")]
    [InlineData("Valid_Name")]
    [InlineData("Valid Name")]
    public void Validator_ShouldNotHaveError_WhenNameIsValid(string validName)
    {
        // Arrange
        var command = new CreateCategoryCommand(new CreateCategoryCommandRequest { Name = validName });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Request.Name);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenDescriptionExceedsMaxLength()
    {
        // Arrange
        var longDescription = new string('a', 1001);
        var command = new CreateCategoryCommand(new CreateCategoryCommandRequest
        {
            Name = "Valid Name",
            Description = longDescription
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Description);
    }

    [Fact]
    public void Validator_ShouldNotHaveError_WhenDescriptionIsValidLength()
    {
        // Arrange
        var validDescription = new string('a', 1000);
        var command = new CreateCategoryCommand(new CreateCategoryCommandRequest
        {
            Name = "Valid Name",
            Description = validDescription
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Request.Description);
    }

    [Fact]
    public void Validator_ShouldNotHaveError_WhenDescriptionIsNull()
    {
        // Arrange
        var command = new CreateCategoryCommand(new CreateCategoryCommandRequest
        {
            Name = "Valid Name",
            Description = null
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Request.Description);
    }

    #endregion Validator Tests
}