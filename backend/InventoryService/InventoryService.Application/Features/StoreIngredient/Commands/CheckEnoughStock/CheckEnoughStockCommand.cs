using InventoryService.Application.Dtos.StoreIngredient;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.StoreIngredient.Commands.CheckEnoughStock
{
    public record class CheckEnoughStockCommand(CheckEnoughStockReq Request) : IRequest<CheckEnoughStockRes>;
}
