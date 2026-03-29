using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Repositories;
using static ProductService.Application.Features.Categories.Queries.GetByCategoryName.GetCategoryByNameQuery;

namespace ProductService.Application.Features.Categories.Queries.GetByCategoryName;

public sealed class GetCategoryByNameQueryHandler(
    ICategoryRepository categoryRepository, IMapper mapper
) : IRequestHandler<GetCategoryByNameQuery, Result<GetCategoryByNameQueryResponse>>
{
    public async Task<Result<GetCategoryByNameQueryResponse>> Handle(
       GetCategoryByNameQuery request,
          CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CategoryName))
        {
            return Result.Invalid(
                new ValidationError("Category name cannot be empty."));
        }
        var category = await categoryRepository.GetByCategoryNameContainingAsync(
            request.CategoryName,
            cancellationToken);
        if (category == null)
        {
            return Result.NotFound($"Category with name '{request.CategoryName}' not found.");
        }

        return Result.Success(mapper.Map<GetCategoryByNameQueryResponse>(category));
    }
}