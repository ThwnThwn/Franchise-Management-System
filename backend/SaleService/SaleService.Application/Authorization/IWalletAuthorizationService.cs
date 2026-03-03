namespace SalesService.Application.Authorization;

using Ardalis.Result;
using SalesService.Domain.Enums;

public interface IWalletAuthorizationService
{
    Result ValidateStatusUpdatePermission(string userRole, WalletStatus newStatus);
}