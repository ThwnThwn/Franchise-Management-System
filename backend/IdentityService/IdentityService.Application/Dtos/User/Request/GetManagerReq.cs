using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Dtos.User.Request
{
    public class GetManagerReq
    {
        [Required]
        public Guid StoreId { get; set; }
    }
}
