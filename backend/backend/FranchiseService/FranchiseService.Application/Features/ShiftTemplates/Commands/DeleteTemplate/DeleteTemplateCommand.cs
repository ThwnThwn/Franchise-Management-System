using MediatR;

namespace FranchiseService.Application.Features.ShiftTemplates.Commands.DeleteTemplate
{
    /// <summary>
    /// Command xóa mẫu ca (Soft Delete).
    /// FE gửi: DELETE /shift-templates/{id}
    /// </summary>
    public record DeleteTemplateCommand(Guid Id) : IRequest<Unit>;
}
