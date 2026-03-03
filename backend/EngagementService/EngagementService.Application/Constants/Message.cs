using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Constants
{
    public static class MembershipTierMsg
    {
        public const string NotFound = "Không tìm thấy hạng thành viên";
        public const string NameExisted = "Tên hạng đã tồn tài";
        public const string MinSpendRequiredExisted = "Số tiền tối thiểu đã tồn tại";

    }

    public static class LoyaltyAccountMsg
    {
        public const string NotFound = "Không tìm thấy tài khoản thành viên";
        public const string InsufficientPoints = "Điểm không đủ để trừ";
        public const string AlreadyExists = "Tài khoản đã tồn tại";
    }

    public static class VoucherMsg
    {
        public const string NotFound = "Không tìm thấy mã giảm giá";
        public const string CodeExisted = "Mã giảm giá đã tồn tại";
        public const string ActiveVoucher = "Không thể Xoá/cập nhật mã giảm giá đang hoạt động";
        public const string InvalidDiscountValue = "Giá trị giảm giá không hợp lệ";
        public const string InvalidEndDate = "Ngày kết thúc không được trước hiện tại";
        public const string InvalidType = "Loại mã giảm giá không hợp lệ";
        public const string EmptyVoucherItems = "Mã giảm giá loại quà tặng phải có ít nhất một voucher item yêu cầu và voucher item quà tặng";
        public const string InvalidVoucherItemQuantity = "Số lượng trong voucher item phải lớn hơn 0";
    }
    public static class CommonMsg
    {
        public const string InvalidId = "Id không hợp lệ";
    }
}
