using AutoMapper;
using Grpc.Core;
using MediatR;
using ProductService.Application.Features.Categories.Command.CreateCategory;
using ProductService.Application.Features.Categories.Command.DeleteCategory;
using ProductService.Application.Features.Categories.Command.UpdateCategory;
using ProductService.Application.Features.Categories.Queries.GetByCategoryName;
using ProductService.Application.Features.Categories.Queries.GetById;
using ProductService.Application.Features.Categories.Queries.GetList;

namespace ProductService.API.Services;

public class CategoryGrpcService : CategoryGrpc.CategoryGrpcBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public CategoryGrpcService(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    public override async Task<GetCategoriesReply> GetCategories(GetCategoriesRequest request, ServerCallContext context)
    {
        var query = new GetCategoriesQuery();
        var result = await _mediator.Send(query, context.CancellationToken);

        if (!result.IsSuccess)
        {
            var errorMessage = result.Errors.FirstOrDefault() ?? "Failed to get categories";
            throw new RpcException(new Status(StatusCode.Internal, errorMessage));
        }

        var reply = new GetCategoriesReply();
        reply.Categories.AddRange(result.Value.Categories.Select(c => new Category
        {
            Id = c.Id.ToString(),
            Name = c.Name,
            Description = c.Description ?? "",
            ImageUrl = c.ImageUrl ?? "",
            IsActive = c.IsActive,
        }));

        return reply;
    }

    public override async Task<DeleteCategoryReply> DeleteCategory(
    DeleteCategoryRequest request,
    ServerCallContext context)
    {
        var command = new DeleteCategoryCommand(Guid.Parse(request.Id));

        var result = await _mediator.Send(command, context.CancellationToken);

        if (!result.IsSuccess)
        {
            throw new RpcException(
                new Status(StatusCode.NotFound,
                result.Errors.FirstOrDefault() ?? "Delete category failed"));
        }

        return new DeleteCategoryReply
        {
            Id = result.Value.Id.ToString(),
            IsDeleted = result.Value.IsDeleted
        };
    }

    public override async Task<UpdateCategoryReply> UpdateCategory(
    UpdateCategoryRequest request,
    ServerCallContext context)
    {
        var command = new UpdateCategoryCommand(
            Guid.Parse(request.Id),
            new UpdateCategoryCommandRequest
            {
                Id = Guid.Parse(request.Id),
                Name = request.Name,
                Description = request.Description
            });

        var result = await _mediator.Send(command, context.CancellationToken);

        if (!result.IsSuccess)
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument,
                result.Errors.FirstOrDefault() ?? "Update failed"));
        }

        return new UpdateCategoryReply
        {
            Id = result.Value.Id.ToString()
        };
    }

    public override async Task<GetCategoryByNameReply> GetCategoryByName(
    GetCategoryByNameRequest request,
    ServerCallContext context)
    {
        var query = new GetCategoryByNameQuery(request.Name);

        var result = await _mediator.Send(query, context.CancellationToken);

        if (!result.IsSuccess)
        {
            throw new RpcException(
                new Status(StatusCode.NotFound,
                result.Errors.FirstOrDefault() ?? "Category not found"));
        }

        var c = result.Value;

        return new GetCategoryByNameReply
        {
            Category = new Category
            {
                Id = c.Id.ToString(),
                Name = c.Name,
                Description = c.Description,
                IsActive = true
            }
        };
    }

    public override async Task<CreateCategoryReply> CreateCategory(
    CreateCategoryRequest request,
    ServerCallContext context)
    {
        var command = new CreateCategoryCommand(
            new CreateCategoryCommand.CreateCategoryCommandRequest
            {
                Name = request.Name,
                Description = request.Description
            });

        var result = await _mediator.Send(command, context.CancellationToken);

        if (!result.IsSuccess)
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument,
                result.Errors.FirstOrDefault() ?? "Create category failed"));
        }

        return new CreateCategoryReply
        {
            Id = result.Value.Id.ToString()
        };
    }

    public override async Task<GetCategoryByIdReply> GetCategoryById(
    GetCategoryByIdRequest request,
    ServerCallContext context)
    {
        var query = new GetCategoryByIdQuery(Guid.Parse(request.Id));

        var result = await _mediator.Send(query, context.CancellationToken);

        if (!result.IsSuccess)
        {
            throw new RpcException(
                new Status(StatusCode.NotFound,
                result.Errors.FirstOrDefault() ?? "Category not found"));
        }

        var c = result.Value;

        return new GetCategoryByIdReply
        {
            Category = new Category
            {
                Id = c.Id.ToString(),
                Name = c.Name,
                Description = c.Description,
                IsActive = true
            }
        };
    }
}