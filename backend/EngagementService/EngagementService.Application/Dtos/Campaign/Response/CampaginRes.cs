using EngagementService.Application.Dtos.Campaign.Request;
using EngagementService.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Dtos.Campaign.Response
{
    public class CampaginRes
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public ICollection<Voucher> Vouchers { get; set; } = [];
    }
    public class Voucher
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public VoucherType Type { get; set; }
        public decimal? DiscountValue { get; set; }
        public decimal? MaxDiscountValue { get; set; }
        public decimal? MinOrderValue { get; set; }
        public decimal? PointCost { get; set; }
        public int? InitQuantity { get; set; }
        public int? CurrentQuantity { get; set; }
        public int? MaxUsagePerUser { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public ICollection<VoucherItem> VoucherItems { get; set; } = new List<VoucherItem>();
    }
    public partial class VoucherItem
    {
        public Guid Id { get; set; }
        public Guid VoucherId { get; set; }
        public string? Sku { get; set; }
        public int Quantity { get; set; }
        public VoucherItemType Type { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
