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
    }

    public static class CommonMsg
    {
        public const string InvalidId = "Id không hợp lệ";
    }
}
