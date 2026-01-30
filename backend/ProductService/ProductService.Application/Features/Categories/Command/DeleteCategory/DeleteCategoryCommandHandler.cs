using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Repositories;
using static ProductService.Application.Features.Categories.Command.DeleteCategory.DeleteCategoryCommand;
namespace ProductService.Application.Features.Categories.Command.DeleteCategory
{
    public sealed class DeleteCategoryCommandHandler
        : IRequestHandler<DeleteCategoryCommand, Result<DeleteCategoryCommandResponse>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<Result<DeleteCategoryCommandResponse>> Handle(
            DeleteCategoryCommand request,
            CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(request.categoryId);

            if (category == null)
            {
                return Result.NotFound($"Category with ID '{request.categoryId}' not found.");
            }

            var hasAssociatedProducts = await _categoryRepository.HasProductsAsync(request.categoryId);
            if (hasAssociatedProducts)
            {
                return Result.Conflict("Cannot delete category with associated products.");
            }
            await _categoryRepository.DeleteAsync(request.categoryId);
            var response = _mapper.Map<DeleteCategoryCommandResponse>(category);

            return Result.Success(response);
        }
    }
}
