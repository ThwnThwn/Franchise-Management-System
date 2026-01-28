using InventoryService.Application.Dtos.StoreIngredient;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.StoreIngredient.Commands.DescreaseStockByOrder
{
    public record class DescreaseStockByOrderCommand(DescreaseStockByOrderReq Request) : IRequest<Unit>;
}
