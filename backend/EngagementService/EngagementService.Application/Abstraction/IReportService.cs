using EngagementService.Application.Dtos.LoyaltyAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Abstraction
{
    public interface IReportService
    {
        byte[] ExportToCsv(LoyaltyReportDto data);
        byte[] ExportToPdf(LoyaltyReportDto data);
    }
}
