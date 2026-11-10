using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Repositories;

namespace ProductService.Application.Features.Categories.Queries.GetList;

public class GetCategoriesHandler(
    ICategoryRepository categoryRepository, IMapper mapper
) : IRequestHandler<GetCategoriesQuery, Result<GetCategoriesResultDto>>
{
    public async Task<Result<GetCategoriesResultDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetAllAsync(cancellationToken);

        var categoryDtos = mapper.Map<List<GetCategoryListItemDto>>(categories);

        var result = new GetCategoriesResultDto
        {
            Categories = categoryDtos
        };

        return Result.Success(result);
    }
}