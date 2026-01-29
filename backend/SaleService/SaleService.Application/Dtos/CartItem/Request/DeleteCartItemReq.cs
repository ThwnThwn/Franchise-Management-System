using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesService.Application.Dtos.CartItem.Request
{
    public class DeleteCartItemReq
    {
        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Sku { get; set; } = null!;
    }
}
