using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Features.Categories.Command.CreateCategory;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;
using static ProductService.Application.Features.Categories.Command.CreateCategory.CreateCategoryCommand;

namespace ProductService.Application.Features.Categories.Commands.Create;
public sealed class CreateCategoryCommandHandler
    : IRequestHandler<CreateCategoryCommand, Result<CreateCategoryCommandResponse>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public CreateCategoryCommandHandler(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<Result<CreateCategoryCommandResponse>> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await _categoryRepository
            .ExistsByNameAsync(request.Request.Name, cancellationToken: cancellationToken);

        if (exists)
        {
            return Result.Conflict(
                $"Category with name '{request.Request.Name}' already exists.");
        }

        var category = _mapper.Map<Category>(request.Request);
        category.Id = Guid.NewGuid();
        await _categoryRepository.AddAsync(category);

        return Result.Success(_mapper.Map<CreateCategoryCommandResponse>(category));
    }
}
