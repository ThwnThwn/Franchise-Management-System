using FluentAssertions;
using Moq;
using MediatR;
using FranchiseService.Application.Features.ShiftTemplates.Commands.DeleteTemplate;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Tests.Features.ShiftTemplates;

public class DeleteTemplateHandlerTests
{
    private readonly Mock<IGenericRepository<ShiftTemplate>> _templateRepoMock;
    private readonly DeleteTemplateHandler _handler;

    public DeleteTemplateHandlerTests()
    {
        _templateRepoMock = new Mock<IGenericRepository<ShiftTemplate>>();
        _handler = new DeleteTemplateHandler(_templateRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFound_WhenTemplateDoesNotExist()
    {
        var id = Guid.NewGuid();
        _templateRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ShiftTemplate?)null);

        var act = () => _handler.Handle(new DeleteTemplateCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact]
    public async Task Handle_ShouldSoftDelete_WhenTemplateExists()
    {
        var templateId = Guid.NewGuid();
        var template = new ShiftTemplate
        {
            Id = templateId,
            Code = "CA-SANG-MATCHA",
            Name = "Ca Sáng Pha Matcha Latte",
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(14, 0)
        };

        _templateRepoMock.Setup(r => r.GetByIdAsync(templateId)).ReturnsAsync(template);

        var result = await _handler.Handle(new DeleteTemplateCommand(templateId), CancellationToken.None);

        result.Should().Be(Unit.Value);
        _templateRepoMock.Verify(r => r.DeleteAsync(templateId), Times.Once);
    }
}
