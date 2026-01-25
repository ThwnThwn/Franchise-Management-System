using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.AppExceptions
{
    public class ConflictDuplicateException : Exception
    {
        public ConflictDuplicateException(string message) : base(message) { }
    }
}
