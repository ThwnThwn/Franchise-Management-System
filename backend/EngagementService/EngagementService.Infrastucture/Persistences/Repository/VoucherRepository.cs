using EngagementService.Application.Constants;
using EngagementService.Application.Dtos.Voucher;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Constants;
using EngagementService.Domain.Entities;
using EngagementService.Infrastucture.AppDbContext;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;

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
                if(updateRequest.DiscountValue <= 0)
                    throw new ArgumentException(VoucherMsg.InvalidDiscountValue);

                if (dbVoucher.Type == VoucherType.DISCOUNT_PERCENTAGE || dbVoucher.Type == VoucherType.SHIPPING_PERCENTAGE)
                {
                    if (updateRequest.DiscountValue > 0 && updateRequest.DiscountValue <= 100)
                        dbVoucher.DiscountValue = updateRequest.DiscountValue;
                }
                else
                {
                    dbVoucher.DiscountValue = updateRequest.DiscountValue;
                }
            }

            if (updateRequest.MaxDiscountValue != null)
            {
                if(updateRequest.MaxDiscountValue <= 0)
                    throw new ArgumentException(VoucherMsg.InvalidMaxDiscountValue);
                dbVoucher.MaxDiscountValue = updateRequest.MaxDiscountValue;
            }
                

            if (updateRequest.PointCost != null)
            {
                if (updateRequest.PointCost < 0)
                    throw new ArgumentException(VoucherMsg.InvalidPointCost);
                dbVoucher.PointCost = updateRequest.PointCost;
            }
                

            if (updateRequest.MinOrderValue != null)
            {
                if (updateRequest.MinOrderValue < 0)
                    throw new ArgumentException(VoucherMsg.InvalidMinOrderValue);
                dbVoucher.MinOrderValue = updateRequest.MinOrderValue;
            }
                

            if (updateRequest.MaxUsagePerUser != null)
            {
                if (updateRequest.MaxUsagePerUser <= 0)
                    throw new ArgumentException(VoucherMsg.InvalidMaxUsagePerUser);
                dbVoucher.MaxUsagePerUser = updateRequest.MaxUsagePerUser;
            }
                
            if (updateRequest.EndDate != null)
            {
                if (updateRequest.EndDate < DateTimeOffset.UtcNow)
                    throw new ArgumentException(VoucherMsg.InvalidEndDate);
                dbVoucher.EndDate = updateRequest.EndDate;
            }
        }
    }
}
