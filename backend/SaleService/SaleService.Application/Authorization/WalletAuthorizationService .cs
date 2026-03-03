
using Ardalis.Result;
using SalesService.Domain.Enums;
namespace SalesService.Application.Authorization;
public sealed class WalletAuthorizationService : IWalletAuthorizationService
{
    public Result ValidateStatusUpdatePermission(string userRole, WalletStatus newStatus)
    {
        // Only Admin and Manager can update wallet status
        if (userRole is not ("Administrator" or "Manager"))
        {
            return Result.Forbidden("Only Admin or Manager can update wallet status");
        }

        // ✅ Additional rules based on status
        if (newStatus == WalletStatus.Suspended)
        {
            // Only Admin can suspend
            if (userRole != "Admin")
            {
                return Result.Forbidden("Only Admin can suspend wallets");
            }
        }

        return Result.Success();
    }
}