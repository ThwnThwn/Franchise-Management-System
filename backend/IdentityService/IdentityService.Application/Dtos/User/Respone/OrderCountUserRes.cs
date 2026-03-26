using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Dtos.User.Respone
{
    public class OrderCountUserRes
    {
        public Guid UserId { get; set; }
        public int OrderCount { get; set; }
    }
}
