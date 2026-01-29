using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Features.Categories.Command.UpdateCategory;
using ProductService.Application.Repositories;

namespace ProductService.Application.Features.Categories.Command.UpdateCategory;

    public sealed class UpdateCategoryCommandHandler
        : IRequestHandler<UpdateCategoryCommand, Result<UpdateCategoryCommandResponse>>
    {
        private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

        public async Task<Result<UpdateCategoryCommandResponse>> Handle(UpdateCategoryCommand request,
            CancellationToken cancellationToken)
        {
            var categoryToUpdate = await _categoryRepository.GetByIdAsync(request.Request.Id);
            if (categoryToUpdate == null)
            {
                return Result.NotFound($" Category with Id '{request.Request.Id}' not found. ");
            }

            var existingCategory = await _categoryRepository.GetByCategoryNameAsync(request.Request.Name);
            if(existingCategory !=null && existingCategory.Id != request.Request.Id)
            {
                return Result.Conflict($" Category name '{request.Request.Name}' is already in use. ");
            }

            
            var categoryUpdate = _mapper.Map(request.Request, categoryToUpdate);
        await _categoryRepository.UpdateAsync(categoryToUpdate);

            

            return Result.Success(_mapper.Map<UpdateCategoryCommandResponse>(categoryUpdate));
        }
    }

