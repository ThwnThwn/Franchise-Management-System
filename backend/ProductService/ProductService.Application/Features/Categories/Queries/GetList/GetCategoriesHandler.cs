using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Repositories;

namespace ProductService.Application.Features.Categories.Queries.GetList
{
    public class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, Result<GetCategoriesResultDto>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public GetCategoriesHandler(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<Result<GetCategoriesResultDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.GetAllAsync(cancellationToken);

            var categoryDtos = _mapper.Map<List<GetCategoryListItemDto>>(categories);

            var result = new GetCategoriesResultDto
            {
                Categories = categoryDtos
            };

            return Result.Success(result);
        }
    }
}