using WarehouseService.Application.Features.Warehouses.Queries.GetById;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Domain.Enums;
using Moq;
using FluentAssertions;
namespace WarehouseService.Application.Tests.Features.Warehouse
{
    public class GetWarehouseQueryHandlerTests
    {
        private readonly Mock<IWarehouseRepository> _repositoryMock;
        private readonly GetWarehouseQueryHandler _handler;

        public GetWarehouseQueryHandlerTests()
        {
            _repositoryMock = new Mock<IWarehouseRepository>();
            _handler = new GetWarehouseQueryHandler(_repositoryMock.Object);
        }

        // -------------------------------------------------------------------------
        // GROUP 1 — NOT FOUND BRANCH
        // -------------------------------------------------------------------------

        [Fact]
        public async Task Handle_ShouldThrowKeyNotFoundException_WhenWarehouseDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            _repositoryMock
                .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Warehouse?)null);

            var query = new GetWarehouseQuery(id);

            // Act & Assert
            await _handler.Invoking(x => x.Handle(query, default))
                .Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"*{id}*");
        }

        // -------------------------------------------------------------------------
        // GROUP 2 — FOUND BRANCH: MAPPING
        // -------------------------------------------------------------------------

        [Fact]
        public async Task Handle_ShouldReturnResult_WithAllFieldsMapped_WhenWarehouseExists()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var createdAt = DateTimeOffset.UtcNow.AddDays(-10);
            var updatedAt = DateTimeOffset.UtcNow.AddDays(-2);

            var warehouse = new Domain.Entities.Warehouse
            {
                Id = Guid.NewGuid(),
                Code = "WH-GET",
                Name = "Get Test Warehouse",
                Address = "2 Query Street",
                StoreId = storeId,
                Status = WarehouseStatus.Active,
                IsDeleted = false,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                WarehouseItems = new List<WarehouseItem>()
            };

            _repositoryMock
                .Setup(x => x.GetByIdAsync(warehouse.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(warehouse);

            var query = new GetWarehouseQuery(warehouse.Id);

            // Act
            var result = await _handler.Handle(query, default);

            // Assert — every field
            result.Should().NotBeNull();
            result.Id.Should().Be(warehouse.Id);
            result.Code.Should().Be("WH-GET");
            result.Name.Should().Be("Get Test Warehouse");
            result.Address.Should().Be("2 Query Street");
            result.StoreId.Should().Be(storeId);
            result.Status.Should().Be(WarehouseStatus.Active);
            result.CreatedAt.Should().Be(createdAt);
            result.UpdatedAt.Should().Be(updatedAt);
        }

        [Fact]
        public async Task Handle_ShouldCallGetByIdAsync_WithCorrectId()
        {
            // Arrange
            var id = Guid.NewGuid();
            var warehouse = new Domain.Entities.Warehouse
            {
                Id = id,
                Code = "WH-X",
                Name = "X",
                Address = "X",
                StoreId = Guid.NewGuid(),
                Status = WarehouseStatus.Active,
                IsDeleted = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                WarehouseItems = new List<WarehouseItem>()
            };

            _repositoryMock
                .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(warehouse);

            var query = new GetWarehouseQuery(id);

            // Act
            await _handler.Handle(query, default);

            // Assert — repository was called with exactly the right Id
            _repositoryMock.Verify(
                x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
    }
}
