using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.AppExceptions
{
    public class ForbidenException : Exception
    {
        public ForbidenException(string message) : base(message) { }
    }
}
