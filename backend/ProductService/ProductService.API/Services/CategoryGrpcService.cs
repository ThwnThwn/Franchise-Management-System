using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using MediatR;
using ProductService.API;
using ProductService.Application.Features.Categories.GetList;

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
        reply.Categories.AddRange(_mapper.Map<List<Category>>(result.Value.Categories));

        return reply;
    }
}
