using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Features.Categories.Queries.GetById;
using ProductService.Application.Repositories;
using static ProductService.Application.Features.Categories.Queries.GetById.GetCategoryByIdQuery;

namespace ProductService.Application.Features.Categories.Queries.GetById
{
    public sealed class GetCategoryByIdQueryHandler :
        IRequestHandler<GetCategoryByIdQuery, Result<GetCategoryByIdResponse>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        public GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<Result<GetCategoryByIdResponse>> Handle(
            GetCategoryByIdQuery request,
            CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(request.categoryId);
            if (category is null)
            {
                return Result.NotFound($"Category with id '{request.categoryId}' not found.");
            }
            
            return Result.Success(_mapper.Map<GetCategoryByIdResponse>(category));
        }
    }
}
