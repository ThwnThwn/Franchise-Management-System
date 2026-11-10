using FluentAssertions;
using Moq;
using FranchiseService.Application.Features.ShiftTemplates.Commands.CreateTemplate;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Tests.Features.ShiftTemplates;

public class CreateTemplateHandlerTests
{
    private readonly Mock<IGenericRepository<ShiftTemplate>> _templateRepoMock;
    private readonly Mock<IGenericRepository<Store>> _storeRepoMock;
    private readonly CreateTemplateHandler _handler;

    public CreateTemplateHandlerTests()
    {
        _templateRepoMock = new Mock<IGenericRepository<ShiftTemplate>>();
        _storeRepoMock = new Mock<IGenericRepository<Store>>();
        _handler = new CreateTemplateHandler(_templateRepoMock.Object, _storeRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateTemplate_WhenStoreIdIsNull_GlobalTemplate()
    {
        var command = new CreateTemplateCommand
        {
            StoreId = null,
            Code = "CA-SANG-ESPRESSO",
            Name = "Ca Sáng Pha Espresso",
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(14, 0),
            BreakDurationMinutes = 30
        };

        _templateRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftTemplate, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<ShiftTemplate>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        _templateRepoMock.Verify(r => r.AddAsync(It.Is<ShiftTemplate>(
            t => t.Code == "CA-SANG-ESPRESSO"
                && t.Name == "Ca Sáng Pha Espresso"
                && t.IsActive == true
                && t.BreakDurationMinutes == 30
        )), Times.Once);
        _storeRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateTemplate_WhenStoreIdValid()
    {
        var storeId = Guid.NewGuid();
        var command = new CreateTemplateCommand
        {
            StoreId = storeId,
            Code = "CA-CHIEU-ROBUSTA",
            Name = "Ca Chiều Rang Robusta",
            StartTime = new TimeOnly(14, 0),
            EndTime = new TimeOnly(22, 0),
            BreakDurationMinutes = 45
        };

        _storeRepoMock
            .Setup(r => r.GetByIdAsync(storeId))
            .ReturnsAsync(new Store
            {
                Id = storeId, Code = "CH-001", Name = "Quán Arabica Bảo Lộc",
                Address = "123 Lê Lợi, Bảo Lộc",
                Shifts = new List<Shift>(), StoreHours = new List<StoreHour>()
            });

        _templateRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftTemplate, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<ShiftTemplate>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        _storeRepoMock.Verify(r => r.GetByIdAsync(storeId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenStoreIdDoesNotExist()
    {
        var storeId = Guid.NewGuid();
        var command = new CreateTemplateCommand
        {
            StoreId = storeId,
            Code = "CA-DEM-LATTE",
            Name = "Ca Đêm Pha Latte",
            StartTime = new TimeOnly(22, 0),
            EndTime = new TimeOnly(6, 0),
            BreakDurationMinutes = 30
        };

        _storeRepoMock
            .Setup(r => r.GetByIdAsync(storeId))
            .ReturnsAsync((Store?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{storeId}*");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOp_WhenCodeAlreadyExistsInStore()
    {
        var storeId = Guid.NewGuid();
        var command = new CreateTemplateCommand
        {
            StoreId = storeId,
            Code = "CA-SANG-ESPRESSO",
            Name = "Ca Sáng Pha Espresso",
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(14, 0),
            BreakDurationMinutes = 30
        };

        _storeRepoMock
            .Setup(r => r.GetByIdAsync(storeId))
            .ReturnsAsync(new Store
            {
                Id = storeId, Code = "CH-002", Name = "Quán Robusta Đà Lạt",
                Address = "456 Trần Phú, Đà Lạt",
                Shifts = new List<Shift>(), StoreHours = new List<StoreHour>()
            });

        _templateRepoMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftTemplate, bool>>>()))
            .ReturnsAsync(new[]
            {
                new ShiftTemplate
                {
                    Id = Guid.NewGuid(), StoreId = storeId,
                    Code = "CA-SANG-ESPRESSO", Name = "Ca Sáng Pha Espresso",
                    StartTime = new TimeOnly(6, 0), EndTime = new TimeOnly(14, 0)
                }
            });

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CA-SANG-ESPRESSO*");
    }
}
