using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesService.Application.Dtos.CartItem.Request
{
    public class DeleteAllCartItemReq
    {
        [Required]
        public Guid CustomerId { get; set; }
    }
}
