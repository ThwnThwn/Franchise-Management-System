using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesService.Application.Dtos.Inventory
{
    public class DecreaseStockByOrderReq
    {
        public Guid StoreId { get; set; }
        public IEnumerable<AmountVariant> Items { get; set; } = [];
    }

    public class AmountVariant
    {
        public Guid VariantId { get; set; }
        public int Amount { get; set; }
    }
}
