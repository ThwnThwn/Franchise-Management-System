using SalesService.Application.Dtos.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesService.Application.ClientInterface.Inventory
{
    public interface IInventoryServiceClient
    {
        Task DecreaseStockByOrder(DecreaseStockByOrderReq request);
    }

    
}
