using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Repositories;
using static ProductService.Application.Features.Categories.Queries.GetById.GetCategoryByIdQuery;

namespace ProductService.Application.Features.Categories.Queries.GetById;

public sealed class GetCategoryByIdQueryHandler(
    ICategoryRepository categoryRepository, IMapper mapper
) : IRequestHandler<GetCategoryByIdQuery, Result<GetCategoryByIdResponse>>
{
    public async Task<Result<GetCategoryByIdResponse>> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.CategoryId);
        if (category is null)
        {
            return Result.NotFound($"Category with id '{request.CategoryId}' not found.");
        }

        return Result.Success(mapper.Map<GetCategoryByIdResponse>(category));
    }
}