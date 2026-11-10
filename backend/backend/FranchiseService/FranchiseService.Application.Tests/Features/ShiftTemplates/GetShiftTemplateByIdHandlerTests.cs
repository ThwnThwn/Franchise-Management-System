using FluentAssertions;
using Moq;
using FranchiseService.Application.Features.ShiftTemplates.Queries.GetShiftTemplateById;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Tests.Features.ShiftTemplates;

public class GetShiftTemplateByIdHandlerTests
{
    private readonly Mock<IGenericRepository<ShiftTemplate>> _templateRepoMock;
    private readonly GetShiftTemplateByIdHandler _handler;

    public GetShiftTemplateByIdHandlerTests()
    {
        _templateRepoMock = new Mock<IGenericRepository<ShiftTemplate>>();
        _handler = new GetShiftTemplateByIdHandler(_templateRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenTemplateDoesNotExist()
    {
        var id = Guid.NewGuid();
        _templateRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ShiftTemplate?)null);

        var act = () => _handler.Handle(new GetShiftTemplateByIdQuery(id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedDto_WhenTemplateExists()
    {
        var id = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow.AddDays(-7);

        var template = new ShiftTemplate
        {
            Id = id,
            StoreId = storeId,
            Code = "CA-CHIEU-CAPPUCCINO",
            Name = "Ca Chiều Pha Cappuccino",
            StartTime = new TimeOnly(14, 0),
            EndTime = new TimeOnly(22, 0),
            BreakDurationMinutes = 30,
            IsActive = true,
            CreatedAt = createdAt
        };

        _templateRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(template);

        var result = await _handler.Handle(new GetShiftTemplateByIdQuery(id), CancellationToken.None);

        result.Id.Should().Be(id);
        result.StoreId.Should().Be(storeId);
        result.Code.Should().Be("CA-CHIEU-CAPPUCCINO");
        result.Name.Should().Be("Ca Chiều Pha Cappuccino");
        result.StartTime.Should().Be(new TimeOnly(14, 0));
        result.EndTime.Should().Be(new TimeOnly(22, 0));
        result.BreakDurationMinutes.Should().Be(30);
        result.IsActive.Should().BeTrue();
        result.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public async Task Handle_ShouldDefaultBreakAndIsActive_WhenNullable()
    {
        var id = Guid.NewGuid();
        var template = new ShiftTemplate
        {
            Id = id,
            Code = "CA-DEM-MOCHA",
            Name = "Ca Đêm Pha Mocha",
            StartTime = new TimeOnly(22, 0),
            EndTime = new TimeOnly(6, 0),
            BreakDurationMinutes = null,
            IsActive = null,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _templateRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(template);

        var result = await _handler.Handle(new GetShiftTemplateByIdQuery(id), CancellationToken.None);

        result.BreakDurationMinutes.Should().Be(0);
        result.IsActive.Should().BeTrue();
    }
}
