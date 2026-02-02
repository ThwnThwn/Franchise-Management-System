using Grpc.Core;
using InventoryService.Application.Features.Ingrdient.Queries.GetAllIngredient;
using InventoryService.Grpc.Protos.Ingredient;
using MediatR;

namespace InventoryService.API.Services.Inventory
{
    public class IngredientGrpcService : IngredientGrpc.IngredientGrpcBase
    {
        private readonly IMediator _mediator;
        public IngredientGrpcService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override async Task<GetAllIngredientRep> GetAllIngredient(GetAllIngredientReq request, ServerCallContext context)
        {
            var res = await _mediator.Send(new GetAllIngredientQuery()
            {
                CategoryId = string.IsNullOrEmpty(request.CategoryId) ? null : Guid.Parse(request.CategoryId),
                Page = request.Page,
                PageSize = request.PageSize
            });
            var response = new GetAllIngredientRep()
            {
                TotalPages = res.TotalPages,
                TotalCount = ((int)res.TotalCount)  
            };

            var protoItems = res.Items.Select(item => new IngredientViewRes
            {
                Id = item.Id.ToString(),
                Name = item.Name,
                BaseUnit = item.BaseUnit,
                CategoryId = item.CategoryId?.ToString() ?? string.Empty
            });

            // 3. Add vào danh sách repeated của Proto
            response.IngredientRes.AddRange(protoItems);

            return response;
        }

        public override async Task<NoneReply> CreateIngredient(CreateIngredientReq request, ServerCallContext context)
        {
            var req = new Application.Features.Ingrdient.Commands.CreateIngredient.CreateIngredientCommand(new()
            {
                Name = request.Name,
                BaseUnit = request.BaseUnit,
                CategoryId = string.IsNullOrEmpty(request.CategoryId) ? null : Guid.Parse(request.CategoryId)
            });

             await _mediator.Send(req);
            return new NoneReply();
        }

        public override async Task<NoneReply> UpdateIngredient(UpdateIngredientReq request, ServerCallContext context)
        {
            var req = new Application.Features.Ingrdient.Commands.UpdateIngredient.UpdateIngredientCommand(new()
            {
                Id = Guid.Parse(request.Id),
                Name = request.Name,
                BaseUnit = request.BaseUnit,
                CategoryId = string.IsNullOrEmpty(request.CategoryId) ? null : Guid.Parse(request.CategoryId)
            });
            await _mediator.Send(req);
            return new NoneReply();
        }

        public override async Task<NoneReply> DeleteIngredient(DeleteIngredientReq request, ServerCallContext context)
        {
            var req = new Application.Features.Ingrdient.Commands.DeleteIngredient.DeleteIngredientCommand(Guid.Parse(request.Id));
            await _mediator.Send(req);
            return new NoneReply();
        }

        
    }
}
