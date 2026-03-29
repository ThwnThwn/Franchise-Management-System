using System.Reflection;
using System.Text.RegularExpressions;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Constants;

public static partial class AppPermissions
{
    // --- ĐỊNH NGHĨA CÁC NHÓM QUYỀN (CONST) ---
    public static class Campaign
    {
        public const string Create = "campaign.create";
        public const string Update = "campaign.update";
        public const string RedeemVoucher = "campaign.voucher.redeem";
    }

    public static class Cart
    {
        public const string Read = "cart.read";
        public const string Create = "cart.create";
        public const string Update = "cart.update";
        public const string Delete = "cart.delete";
    }

    public static class Category
    {
        public const string Create = "category.create";
        public const string Update = "category.update";
        public const string Delete = "category.delete";
    }

    public static class FranchiseApplication
    {
        public const string Submit = "franchise.application.submit";
        public const string Read = "franchise.application.read";
        public const string Review = "franchise.application.review";
    }

    public static class FranchiseContract
    {
        public const string Read = "franchise.contract.read";
        public const string Payment = "franchise.contract.payment";
        public const string Setup = "franchise.contract.setup";
        public const string Write = "franchise.contract.write";
    }

    public static class Ingredient
    {
        public const string Read = "ingredient.read";
        public const string Create = "ingredient.create";
        public const string Update = "ingredient.update";
        public const string Delete = "ingredient.delete";
    }

    public static class IngredientCategory
    {
        public const string Read = "ingredient.category.read";
        public const string Create = "ingredient.category.create";
        public const string Update = "ingredient.category.update";
        public const string Delete = "ingredient.category.delete";
    }

    public static class LoyaltyAccount
    {
        public const string Read = "loyalty-account.read";
    }

    public static class LoyaltyTier
    {
        public const string Create = "loyalty-tier.create";
        public const string Update = "loyalty-tier.update";
        public const string Delete = "loyalty-tier.delete";
    }

    public static class Me
    {
        public const string CurrentShiftRead = "me.current-shift.read";
        public const string CheckIn = "me.check-in";
        public const string CheckOut = "me.check-out";
        public const string AgendaRead = "me.agenda.read";
        public const string TimeLogHistoryRead = "me.time-log-history.read";
        public const string ShiftAvailabilityRead = "me.shift.availability.read";
        public const string ShiftSwapRead = "me.shift.swap.read";
    }

    public static class Order
    {
        public const string Read = "order.read";
        public const string Create = "order.create";
        public const string Update = "order.update";
        public const string Payment = "order.payment";
        public const string ReportRead = "order.report.read";
        public const string DeliveryAssign = "order.delivery.assign";
    }

    public static class Payment
    {
        public const string Read = "payment.read";
        public const string ReadAll = "payment.read.all";
    }

    public static class RolePermission
    {
        public const string Read = "permission.read";
        public const string Update = "permission.update";
    }

    public static class Product
    {
        public const string Create = "product.create";
        public const string Update = "product.update";
        public const string Delete = "product.delete";
    }

    public static class Refund
    {
        public const string Read = "wallet.refund.read";
        public const string Approve = "wallet.refund.approve";
        public const string Update = "wallet.refund.update";
        public const string ReadMe = "wallet.refund.read.me";
        public const string CreateMe = "wallet.refund.create.me";
    }

    public static class Role
    {
        public const string Create = "role.create";
        public const string Update = "role.update";
        public const string ReadPermission = "role.permission.read";
        public const string UpdatePermission = "role.permission.update";
    }

    public static class Shift
    {
        public const string Read = "shift.read";
        public const string Write = "shift.write";
        public const string Delete = "shift.delete";
        public const string Publish = "shift.publish";
        public const string ReleaseWrite = "shift.release.write";
    }

    public static class ShiftApplication
    {
        public const string Read = "shift.application.read";
        public const string Write = "shift.application.write";
    }

    public static class ShiftSwap
    {
        public const string Read = "shift.swap.read";
        public const string Write = "shift.swap.write";
        public const string Approve = "shift.swap.approve";
        public const string Cancel = "shift.swap.cancel";
    }

    public static class ShiftTimeLog
    {
        public const string Read = "shift.time-log.read";
        public const string Write = "shift.time-log.write";
    }

    public static class ShiftTemplate
    {
        public const string Read = "shift.template.read";
        public const string Write = "shift.template.write";
        public const string Delete = "shift.template.delete";
    }

    public static class ShiftRule
    {
        public const string Read = "shift.rule.read";
        public const string Write = "shift.rule.write";
    }

    public static class ShiftAvailability
    {
        public const string Read = "shift.availability.read";
        public const string Create = "shift.availability.create";
        public const string Update = "shift.availability.update";
        public const string Delete = "shift.availability.delete";
    }

    public static class Store
    {
        public const string StaffRead = "store.staff.read";
    }

    public static class Stock
    {
        public const string Read = "stock.read";
        public const string Update = "stock.update";
    }

    public static class Supplier
    {
        public const string Read = "supplier.read";
        public const string Create = "supplier.create";
        public const string Update = "supplier.update";
        public const string Delete = "supplier.delete";
    }

    public static class SupplierPayment
    {
        public const string Read = "supplier.payment.read";
        public const string Create = "supplier.payment.create";
    }

    public static class PurchaseOrder
    {
        public const string Read = "supplier.po.read";
        public const string Create = "supplier.po.create";
        public const string Update = "supplier.po.update";
        public const string Complete = "supplier.po.complete";
    }

    public static class User
    {
        public const string Read = "user.read";
        public const string Create = "user.create";
        public const string Update = "user.update";
    }

    public static class Recipe
    {
        public const string Update = "recipe.update";
    }

    public static class Voucher
    {
        public const string Create = "voucher.create";
        public const string Update = "voucher.update";
        public const string Delete = "voucher.delete";
        public const string ReadMe = "voucher.me.read";
        public const string Redeem = "voucher.redeem";
    }

    public static class Wallet
    {
        public const string Read = "wallet.read";
        public const string Update = "wallet.update";
        public const string ReadMe = "wallet.read.me";
        public const string TopupMe = "wallet.topup.me";
    }

    public static class WalletTransaction
    {
        public const string Read = "wallet.transaction.read";
        public const string ReadMe = "wallet.transaction.read.me";
    }

    public static class Withdrawal
    {
        public const string Read = "wallet.withdrawal.read";
        public const string Approve = "wallet.withdrawal.approve";
        public const string Update = "wallet.withdrawal.update";
        public const string ReadMe = "wallet.withdrawal.read.me";
        public const string CreateMe = "wallet.withdrawal.create.me";
        public const string UpdateMe = "wallet.withdrawal.update.me";
    }

    // --- TỰ ĐỘNG QUÉT (REFLECTION) ---
    // Biến này sẽ được dùng cho SyncService
    public static readonly IReadOnlyList<Permission> AllPermissions = GetAllPermissions();

    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex SpaceBeforeUpperRegex();

    private static List<Permission> GetAllPermissions()
    {
        var permissions = new List<Permission>();

        // Quét các nested class (User, Role...)
        foreach (var type in typeof(AppPermissions).GetNestedTypes())
        {
            var groupName = type.Name; // Ví dụ: "User"

            // Quét các field const string
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            {
                if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                {
                    var slug = (string)field.GetValue(null)!;
                    var name = field.Name;
                    var resourceName = SpaceBeforeUpperRegex().Replace(groupName, "$1 $2");

                    permissions.Add(new Permission
                    {
                        Slug = slug, // "user.read"
                        Resource = resourceName, // "User"
                        Name = $"{name} {groupName}", // "Read User"
                        Description = $"Allow {name.ToLower()} {groupName.ToLower()}" // "Allow read user"
                    });
                }
            }
        }
        return permissions;
    }
}