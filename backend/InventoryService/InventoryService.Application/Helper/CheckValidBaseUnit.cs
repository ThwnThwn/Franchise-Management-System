using InventoryService.Application.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Helper
{
    public static class CheckValidBaseUnit
    {
        private static readonly HashSet<string> _validUnits = new(StringComparer.OrdinalIgnoreCase)
        {
            BaseUnitConstants.Gram, BaseUnitConstants.Mililit, BaseUnitConstants.Piece
        };

        public static bool IsValid(string unit)
        {
            return !string.IsNullOrWhiteSpace(unit) && _validUnits.Contains(unit);
        }
    }
}
