using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Repositories;
using static ProductService.Application.Features.Categories.Command.DeleteCategory.DeleteCategoryCommand;

namespace ProductService.Application.Features.Categories.Command.DeleteCategory;

public sealed class DeleteCategoryCommandHandler(
    ICategoryRepository categoryRepository, IMapper mapper
) : IRequestHandler<DeleteCategoryCommand, Result<DeleteCategoryCommandResponse>>
{
    public async Task<Result<DeleteCategoryCommandResponse>> Handle(
        DeleteCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.CategoryId);

        if (category == null)
        {
            return Result.NotFound($"Category with ID '{request.CategoryId}' not found.");
        }

        var hasAssociatedProducts = await categoryRepository.HasProductsAsync(request.CategoryId, cancellationToken);

        if (hasAssociatedProducts)
        {
            return Result.Conflict("Cannot delete category with associated products.");
        }
        await categoryRepository.DeleteAsync(request.CategoryId);
        var response = mapper.Map<DeleteCategoryCommandResponse>(category);

        return Result.Success(response);
    }
}