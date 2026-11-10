using Ardalis.Result;
using MediatR;
using static ProductService.Application.Features.Categories.Command.DeleteCategory.DeleteCategoryCommand;

namespace ProductService.Application.Features.Categories.Command.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid CategoryId)
    : IRequest<Result<DeleteCategoryCommandResponse>>
{
    public sealed class DeleteCategoryCommandResponse
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset DeletedAt { get; set; }
    }
}