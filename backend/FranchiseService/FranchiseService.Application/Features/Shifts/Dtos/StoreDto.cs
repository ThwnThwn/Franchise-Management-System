using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FranchiseService.Application.Features.Shifts.Dtos
{
    public class UserShiftDto
    {
        public bool HasShiftToday { get; set; }
        public Guid? StoreId { get; set; }
        public string? StoreName { get; set; }
        public string? ShiftStart { get; set; }
        public string? ShiftEnd { get; set; }
        public double StoreLat { get; set; }
        public double StoreLong { get; set; }
    }
}