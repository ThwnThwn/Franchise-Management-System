using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using InventoryService.Application.Features.Category.Commands.CreateCategory;
using InventoryService.Application.Features.Category.Commands.DeleteCategory;
using InventoryService.Application.Features.Category.Commands.UpdateCategory;
using InventoryService.Application.Features.Category.Queries.GetAllCategory;
using InventoryService.Grpc.Protos.IngredientCategory;
using MediatR;

namespace InventoryService.API.Services.IngredientCategory
{
    public class CategoryGrpcService : IngredientCategoryGrpc.IngredientCategoryGrpcBase
    {
        private readonly IMediator _mediator;
        public CategoryGrpcService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override async Task<Empty> CreateCategory(CreateCategoryRequest request, ServerCallContext context)
        {
            var obj = new Application.Dtos.Category.CreateCategoryRequest
            {
                Name = request.Name,
                Description = request.Description
            };
            var req = new CreateCategoryCommand(obj);
            await _mediator.Send(req);
            return new Empty();
        }

        public override async Task<Empty> UpdateCategory(UpdateCategoryRequest request, ServerCallContext context)
        {
            var obj = new Application.Dtos.Category.UpdateCategoryRequest
            {
                Id = Guid.Parse(request.Id),
                Name = request.Name,
                Description = request.Description
            };
            var req = new UpdateCategoryCommand(obj);
            await _mediator.Send(req);
            return new Empty();
        }

        public override async Task<Empty> DeleteCategory(DeleteCategoryRequest request, ServerCallContext context)
        { 
            await _mediator.Send(new DeleteCategoryCommand(Guid.Parse(request.Id)));
            return new Empty();
        }

        public override async Task<GetAllCategoriesResponse> GetAllCategories(Empty request, ServerCallContext context)
        {
            var categories = await _mediator.Send(new GetAllCategoryQuery());
            var response = new GetAllCategoriesResponse();
            response.Categories.AddRange(categories.Select(c => new CategoryViewRes
            {
                Id = c.Id.ToString(),
                Name = c.Name,
                Description = c.Description ?? ""
            }));
            return response;
        }
    }
}
