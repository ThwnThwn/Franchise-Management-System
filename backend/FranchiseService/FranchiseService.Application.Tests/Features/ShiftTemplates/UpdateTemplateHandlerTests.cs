using FluentAssertions;
using Moq;
using MediatR;
using FranchiseService.Application.Features.ShiftTemplates.Commands.UpdateTemplate;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Tests.Features.ShiftTemplates;

public class UpdateTemplateHandlerTests
{
    private readonly Mock<IGenericRepository<ShiftTemplate>> _templateRepoMock;
    private readonly UpdateTemplateHandler _handler;

    public UpdateTemplateHandlerTests()
    {
        _templateRepoMock = new Mock<IGenericRepository<ShiftTemplate>>();
        _handler = new UpdateTemplateHandler(_templateRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenTemplateDoesNotExist()
    {
        var id = Guid.NewGuid();
        var command = new UpdateTemplateCommand { Id = id, Name = "Ca Mới" };

        _templateRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ShiftTemplate?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact]
    public async Task Handle_ShouldOnlyUpdateProvidedFields_PartialUpdate()
    {
        var templateId = Guid.NewGuid();
        var existing = new ShiftTemplate
        {
            Id = templateId,
            Code = "CA-SANG-ARABICA",
            Name = "Ca Sáng Arabica Cầu Đất",
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(14, 0),
            BreakDurationMinutes = 30,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-10),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-5)
        };

        _templateRepoMock.Setup(r => r.GetByIdAsync(templateId)).ReturnsAsync(existing);

        var command = new UpdateTemplateCommand
        {
            Id = templateId,
            Name = "Ca Sáng Arabica Đà Lạt"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
        existing.Name.Should().Be("Ca Sáng Arabica Đà Lạt");
        existing.Code.Should().Be("CA-SANG-ARABICA");
        existing.StartTime.Should().Be(new TimeOnly(6, 0));
        existing.EndTime.Should().Be(new TimeOnly(14, 0));
        existing.BreakDurationMinutes.Should().Be(30);
        existing.IsActive.Should().BeTrue();

        _templateRepoMock.Verify(r => r.UpdateAsync(existing), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateAllFields_WhenAllProvided()
    {
        var templateId = Guid.NewGuid();
        var existing = new ShiftTemplate
        {
            Id = templateId,
            Code = "CA-CHIEU-ROBUSTA",
            Name = "Ca Chiều Robusta Buôn Ma Thuột",
            StartTime = new TimeOnly(14, 0),
            EndTime = new TimeOnly(22, 0),
            BreakDurationMinutes = 30,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-10),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-5)
        };

        _templateRepoMock.Setup(r => r.GetByIdAsync(templateId)).ReturnsAsync(existing);

        var command = new UpdateTemplateCommand
        {
            Id = templateId,
            Code = "CA-DEM-ROBUSTA",
            Name = "Ca Đêm Robusta Đắk Lắk",
            StartTime = new TimeOnly(22, 0),
            EndTime = new TimeOnly(6, 0),
            BreakDurationMinutes = 60,
            IsActive = false
        };

        await _handler.Handle(command, CancellationToken.None);

        existing.Code.Should().Be("CA-DEM-ROBUSTA");
        existing.Name.Should().Be("Ca Đêm Robusta Đắk Lắk");
        existing.StartTime.Should().Be(new TimeOnly(22, 0));
        existing.EndTime.Should().Be(new TimeOnly(6, 0));
        existing.BreakDurationMinutes.Should().Be(60);
        existing.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldNotChangeOtherFields_WhenOnlyCodeUpdated()
    {
        var templateId = Guid.NewGuid();
        var existing = new ShiftTemplate
        {
            Id = templateId,
            Code = "CA-SANG-001",
            Name = "Ca Sáng Pha Sữa Đặc",
            StartTime = new TimeOnly(7, 0),
            EndTime = new TimeOnly(15, 0),
            BreakDurationMinutes = 45,
            IsActive = true
        };

        _templateRepoMock.Setup(r => r.GetByIdAsync(templateId)).ReturnsAsync(existing);

        var command = new UpdateTemplateCommand
        {
            Id = templateId,
            Code = "CA-SANG-002"
        };

        await _handler.Handle(command, CancellationToken.None);

        existing.Code.Should().Be("CA-SANG-002");
        existing.Name.Should().Be("Ca Sáng Pha Sữa Đặc");
        existing.IsActive.Should().BeTrue();
    }
}
