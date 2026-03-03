using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Constants
{
    public enum ExportFormat
    {
        View, // Chỉ xem dữ liệu JSON
        Pdf,
        Csv,
        Excel // Dễ dàng thêm vào sau này
    }
}
