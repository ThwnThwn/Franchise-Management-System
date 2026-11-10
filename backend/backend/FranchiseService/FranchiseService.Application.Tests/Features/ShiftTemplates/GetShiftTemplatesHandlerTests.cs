using FluentAssertions;
using Moq;
using FranchiseService.Application.Features.ShiftTemplates.Queries.GetShiftTemplates;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Tests.Features.ShiftTemplates;

public class GetShiftTemplatesHandlerTests
{
    private readonly Mock<IGenericRepository<ShiftTemplate>> _templateRepoMock;
    private readonly Mock<IGenericRepository<Store>> _storeRepoMock;
    private readonly GetShiftTemplatesHandler _handler;

    public GetShiftTemplatesHandlerTests()
    {
        _templateRepoMock = new Mock<IGenericRepository<ShiftTemplate>>();
        _storeRepoMock = new Mock<IGenericRepository<Store>>();
        _handler = new GetShiftTemplatesHandler(_templateRepoMock.Object, _storeRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenStoreIdDoesNotExist()
    {
        var storeId = Guid.NewGuid();
        _storeRepoMock.Setup(r => r.GetByIdAsync(storeId)).ReturnsAsync((Store?)null);

        var query = new GetShiftTemplatesQuery(storeId, null, null);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{storeId}*");
    }

    [Fact]
    public async Task Handle_ShouldReturnAllTemplates_WhenNoFilter()
    {
        var templates = new List<ShiftTemplate>
        {
            new()
            {
                Id = Guid.NewGuid(), Code = "CA-SANG-ESPRESSO",
                Name = "Ca Sáng Pha Espresso", StoreId = null,
                StartTime = new TimeOnly(6, 0), EndTime = new TimeOnly(14, 0),
                BreakDurationMinutes = 30, IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(), Code = "CA-CHIEU-LATTE",
                Name = "Ca Chiều Pha Latte", StoreId = null,
                StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(22, 0),
                BreakDurationMinutes = null, IsActive = null,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        _templateRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftTemplate, bool>>>()))
            .ReturnsAsync(templates);

        var query = new GetShiftTemplatesQuery(null, null, null);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Code.Should().Be("CA-SANG-ESPRESSO");
        result[1].BreakDurationMinutes.Should().Be(0);
        result[1].IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldSkipStoreValidation_WhenStoreIdIsNull()
    {
        _templateRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftTemplate, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<ShiftTemplate>());

        var query = new GetShiftTemplatesQuery(null, null, null);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
        _storeRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldValidateStoreAndReturnTemplates_WhenStoreIdValid()
    {
        var storeId = Guid.NewGuid();

        _storeRepoMock.Setup(r => r.GetByIdAsync(storeId))
            .ReturnsAsync(new Store
            {
                Id = storeId, Code = "CH-ARABICA-01", Name = "Quán Arabica Cầu Đất",
                Address = "Cầu Đất, Đà Lạt",
                Shifts = new List<Shift>(), StoreHours = new List<StoreHour>()
            });

        _templateRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftTemplate, bool>>>()))
            .ReturnsAsync(new List<ShiftTemplate>
            {
                new()
                {
                    Id = Guid.NewGuid(), Code = "CA-SANG",
                    Name = "Ca Sáng Xay Robusta",
                    StoreId = storeId,
                    StartTime = new TimeOnly(6, 0), EndTime = new TimeOnly(14, 0),
                    BreakDurationMinutes = 30, IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            });

        var query = new GetShiftTemplatesQuery(storeId, null, null);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().ContainSingle();
        result[0].StoreId.Should().Be(storeId);
        _storeRepoMock.Verify(r => r.GetByIdAsync(storeId), Times.Once);
    }
}
