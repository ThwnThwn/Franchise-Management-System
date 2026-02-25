using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Dtos.Product
{
    public class ProductVariantRespone
    {
        public Guid Id { get; set; }
        public string Sku { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Attributes { get; set; } = "";
        public decimal BasePrice { get; set; }
        public decimal Cost { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public Guid ProductId { get; set; }
    }


}
