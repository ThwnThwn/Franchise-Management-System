using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SalesService.Application.Dtos.CartItem.Request
{
    public class DeleteListCartItemReq
    {
        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Sku list cannot be empty")]
        public List<string> Skus { get; set; } = new();
    }
}
