using EngagementService.Domain.Constants;
using EngagementService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Dtos.Voucher
{
    public class CreateVoucherRequest
    {
        public string Code { get; set; } = null!;

        public VoucherType Type { get; set; }

        public decimal? DiscountValue { get; set; }

        public decimal? MaxDiscountValue { get; set; }

        public decimal? MinOrderValue { get; set; }

        public decimal? PointCost { get; set; }

        public int? InitQuantity { get; set; }

        public int? MaxUsagePerUser { get; set; }

        public DateTimeOffset? EndDate { get; set; }
        public virtual IEnumerable<CreateVoucherItemReq> VoucherItems { get; set; } = new List<CreateVoucherItemReq>();
    }

    public class CreateVoucherItemReq
    {
        public Guid VariantId { get; set; }

        public int Quantity { get; set; }

        public VoucherItemType Type { get; set; }
    }

    public class UpdateVoucherRequest
    {
        public Guid Id { get; set; }

        //public bool? IsActive { get; set; }
        public decimal? DiscountValue { get; set; }

        public decimal? MaxDiscountValue { get; set; }

        public decimal? MinOrderValue { get; set; }

        public decimal? PointCost { get; set; }

        public int? MaxUsagePerUser { get; set; }

        public DateTimeOffset? EndDate { get; set; }
        public virtual IEnumerable<UpdateVoucherItemReq> VoucherItems { get; set; } = new List<UpdateVoucherItemReq>();
    }

    public class UpdateVoucherItemReq
    {
        public Guid? Id { get; set; }
        public Guid? VariantId { get; set; }
        public int? Quantity { get; set; }
        public VoucherItemType? Type { get; set; }
    }

    public class UpdateVoucherStatusRequest
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }
    }
}
