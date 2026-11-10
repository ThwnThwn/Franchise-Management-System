using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Domain.Commons
{
    public abstract class SoftDeletedEntity
    {
        public bool IsDeleted { get; set; }
    }
}