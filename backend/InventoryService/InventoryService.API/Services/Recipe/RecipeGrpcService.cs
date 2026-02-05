using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using InventoryService.Application.Features.Recipe.Commands.MutationRecipe;
using InventoryService.Application.Features.Recipe.Queries.GetByVariantIds;
using InventoryService.Grpc.Protos.Recipe;
using MediatR;

namespace InventoryService.API.Services.Recipe
{
    public class RecipeGrpcService : RecipeGrpc.RecipeGrpcBase
    {
        private readonly IMediator _mediator;
        public RecipeGrpcService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override async Task<GetByVariantIdsResponse> GetByVariantIds(GetByVariantIdsRequest request, ServerCallContext context)
        {
            var query = new GetByVariantIdsQuery(request.VariantIds.Select(Guid.Parse).ToList());

            var recipes = await _mediator.Send(query);
            var response = new GetByVariantIdsResponse();
            response.Items.AddRange(recipes.Select(r => new RecipeRep
            {
                VariantId = r.VariantId.ToString(),
                Quantity = (double)r.Quantity,
                Name = r.Name,
                BaseUnit = r.BaseUnit
            }));
            return response;
        }

        public override async Task<Empty> Mutation(MutationRecipeRequest request, ServerCallContext context)
        {
            var command = new MutationRecipeCommand(new()
            {
                VariantId = Guid.Parse(request.VariantId),
                Ingredients = request.Ingredients.Select(i => new Application.Dtos.Recipe.RecipeIngredientRequest
                {
                    IngredientId = Guid.Parse(i.IngredientId),
                    Quantity = (decimal)i.Quantity
                }).ToList()
            });
            await _mediator.Send(command);
            return new Empty();
        }

    }
}
