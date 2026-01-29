using InventoryService.Application.Dtos.StoreIngredient;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.StoreIngredient.Commands.DecreaseStockByOrder
{
    public record class DecreaseStockByOrderCommand(DecreaseStockByOrderReq Request) : IRequest<Unit>;
}
