using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesService.Application.Dtos.CartItem.Request
{
    public class CreateCartItemReq
    {
        [Required]
        public string Sku { get; set; } = default!;
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Required]
        public decimal BasePrice { get; set; }

        [Required]
        public Guid CustomerId { get; set; }
    }
}
