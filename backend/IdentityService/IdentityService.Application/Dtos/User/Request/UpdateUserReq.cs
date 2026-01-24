using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Dtos.User.Request
{
    public class UpdateUserReq
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }

        public string? Address { get; set; }

        public bool? IsBlocked { get; set; }

        public Guid? RoleId { get; set; }
    }
}
