using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;
using static ProductService.Application.Features.Categories.Command.CreateCategory.CreateCategoryCommand;

namespace ProductService.Application.Features.Categories.Command.CreateCategory;

public sealed class CreateCategoryCommandHandler(
    ICategoryRepository categoryRepository, IMapper mapper
) : IRequestHandler<CreateCategoryCommand, Result<CreateCategoryCommandResponse>>
{
    public async Task<Result<CreateCategoryCommandResponse>> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await categoryRepository
            .ExistsByNameAsync(request.Request.Name, cancellationToken: cancellationToken);

        if (exists)
        {
            return Result.Conflict(
                $"Category with name '{request.Request.Name}' already exists.");
        }

        var category = mapper.Map<Category>(request.Request);
        category.Id = Guid.NewGuid();
        await categoryRepository.AddAsync(category);

        return Result.Success(mapper.Map<CreateCategoryCommandResponse>(category));
    }
}