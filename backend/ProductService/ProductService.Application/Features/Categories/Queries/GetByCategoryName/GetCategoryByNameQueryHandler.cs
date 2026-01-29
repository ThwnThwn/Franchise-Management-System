using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Repositories;
using static ProductService.Application.Features.Categories.Queries.GetByCategoryName.GetCategoryByNameQuery;
namespace ProductService.Application.Features.Categories.Queries.GetByCategoryName
{
    public sealed class GetCategoryByNameQueryHandler :
        IRequestHandler<GetCategoryByNameQuery, Result<GetCategoryByNameQueryResponse>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        public GetCategoryByNameQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<Result<GetCategoryByNameQueryResponse>> Handle(
           GetCategoryByNameQuery request,
              CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.categoryName))
            {
                return Result.Invalid(
                    new ValidationError("Category name cannot be empty."));
            }
            var category = await _categoryRepository.GetByCategoryNameAsync(
                request.categoryName,
                cancellationToken);
            if (category == null)
            {
                return Result.NotFound($"Category with name '{request.categoryName}' not found.");
            }
            
            return Result.Success(_mapper.Map<GetCategoryByNameQueryResponse>(category) );
        }
    }
}
