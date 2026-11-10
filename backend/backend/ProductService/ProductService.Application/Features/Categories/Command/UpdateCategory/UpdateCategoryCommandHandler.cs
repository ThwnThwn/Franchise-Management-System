using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Repositories;

namespace ProductService.Application.Features.Categories.Command.UpdateCategory;

public sealed class UpdateCategoryCommandHandler(
    ICategoryRepository categoryRepository, IMapper mapper
) : IRequestHandler<UpdateCategoryCommand, Result<UpdateCategoryCommandResponse>>
{
    public async Task<Result<UpdateCategoryCommandResponse>> Handle(UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var categoryToUpdate = await categoryRepository.GetByIdAsync(request.Request.Id);
        if (categoryToUpdate == null)
        {
            return Result.NotFound($" Category with Id '{request.Request.Id}' not found. ");
        }

        var existingCategory = await categoryRepository.GetByCategoryNameAsync(request.Request.Name, cancellationToken);
        if (existingCategory != null && existingCategory.Id != request.Request.Id)
        {
            return Result.Conflict($" Category name '{request.Request.Name}' is already in use. ");
        }

        var categoryUpdate = mapper.Map(request.Request, categoryToUpdate);
        if (string.IsNullOrEmpty(request.Request.Name))
        {
            categoryUpdate.Name = categoryToUpdate.Name;
        }
        await categoryRepository.UpdateAsync(categoryToUpdate);

        return Result.Success(mapper.Map<UpdateCategoryCommandResponse>(categoryUpdate));
    }
}