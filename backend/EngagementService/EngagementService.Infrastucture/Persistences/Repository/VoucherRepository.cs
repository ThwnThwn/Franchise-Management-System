using EngagementService.Application.Constants;
using EngagementService.Application.Dtos.Voucher;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Constants;
using EngagementService.Domain.Entities;
using EngagementService.Infrastucture.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace EngagementService.Infrastucture.Persistences.Repository
{
    public class VoucherRepository : GenericRepository<Voucher>, IVoucherRepository
    {
        public VoucherRepository(IEngagementDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Voucher?> GetVoucherByCodeAsync(string code)
        {
            return await _dbContext.Vouchers
                 .Where(v => v.Code == code)
                 .FirstOrDefaultAsync();
        }

        public async Task UpdateVoucherAsync(UpdateVoucherRequest voucherUpdate)
        {
            var voucherDb = _dbContext.Vouchers.Where(v => v.Id == voucherUpdate.Id)
                .Include(x => x.VoucherItems)
                .FirstOrDefault();
            if (voucherDb == null)
                throw new ArgumentException(VoucherMsg.NotFound);

            if(voucherDb.IsActive)
                throw new ArgumentException(VoucherMsg.ActiveVoucher);

            UpdateVoucher(voucherDb, voucherUpdate);
            if (voucherDb.Type == VoucherType.GIFT && (voucherUpdate.VoucherItems != null || voucherUpdate.VoucherItems!.Count() > 0))
            {
                UpdateVoucherItem(voucherDb, voucherUpdate.VoucherItems!);
            }
            await _dbContext.SaveChangesAsync();
        }

        private void UpdateVoucherItem(Voucher dbVoucher, IEnumerable<UpdateVoucherItemReq> updateRequests)
        {
            var dbVoucherItems = dbVoucher.VoucherItems;
            foreach (var item in dbVoucherItems)
            {
                if (!updateRequests.Any(r => r.Id == item.Id))
                {
                    item.IsDeleted = true;
                }
            }

            foreach (var item in updateRequests)
            {
                var voucherItem = dbVoucherItems.Where(r => r.Id == item.Id).FirstOrDefault();
                if (voucherItem != null)
                {
                    if (item.Quantity != null && item.Quantity > 0)
                        voucherItem.Quantity = (int)item.Quantity;
                    if (item.VariantId != null)
                        voucherItem.VariantId = (Guid)item.VariantId;
                    if (item.Type != null) ;
                        voucherItem.Type = (VoucherItemType)item.Type!;
                }
                else
                {
                    var newItem = new VoucherItem()
                    {
                        Quantity = (int)item.Quantity!,
                        VariantId = (Guid)item.VariantId!,
                        Type = (VoucherItemType)item.Type!,
                        VoucherId = dbVoucher.Id
                    };
                    dbVoucherItems.Add(newItem);
                }
            }
        }
        private void UpdateVoucher(Voucher dbVoucher, UpdateVoucherRequest updateRequest)
        {
            if (updateRequest.DiscountValue != null)
            {
                if (dbVoucher.Type == VoucherType.DISCOUNT_PERCENTAGE || dbVoucher.Type == VoucherType.SHIPPING_PERCENTAGE)
                {
                    if (updateRequest.DiscountValue > 0 && updateRequest.DiscountValue <= 100)
                        updateRequest.DiscountValue = updateRequest.DiscountValue;
                }
                else
                {
                    updateRequest.DiscountValue = updateRequest.DiscountValue;
                }
            }

            if (updateRequest.MaxDiscountValue != null)
                dbVoucher.MaxDiscountValue = updateRequest.MaxDiscountValue;

            if (updateRequest.PointCost != null)
                dbVoucher.PointCost = updateRequest.PointCost;

            if (updateRequest.MinOrderValue != null)
                dbVoucher.MinOrderValue = updateRequest.MinOrderValue;

            if (updateRequest.MaxUsagePerUser != null)
                dbVoucher.MaxUsagePerUser = updateRequest.MaxUsagePerUser;

            if (updateRequest.EndDate != null)
                dbVoucher.EndDate = updateRequest.EndDate;
        }
    }
}
