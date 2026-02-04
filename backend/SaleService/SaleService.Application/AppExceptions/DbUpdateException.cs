using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesService.Application.AppExceptions
{
    public class DbUpdateException : Exception
    {
        public DbUpdateException(string message) : base(message) { }
    }
}
