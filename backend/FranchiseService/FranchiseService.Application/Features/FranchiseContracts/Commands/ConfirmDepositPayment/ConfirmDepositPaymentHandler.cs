using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FranchiseService.Application.Features.FranchiseContracts.Commands.ConfirmDepositPayment;

/// <summary>
/// Command: Kế toán xác nhận đã nhận thanh toán tiền cọc/phí nhượng quyền.
/// Chuyển ContractStatus: AwaitingPayment → Active
/// </summary>
public class ConfirmDepositPaymentCommand : IRequest<bool>
{
    public Guid ContractId { get; set; }
    public Guid ConfirmedBy { get; set; }
    public string? Note { get; set; }
}

public class ConfirmDepositPaymentHandler(
    IFranchiseUow uow,
    IFranchiseContractRepository contractRepo,
    IStoreRepository storeRepo,
    IIdentityServiceClient identityServiceClient,
    ILogger<ConfirmDepositPaymentHandler> logger
) : IRequestHandler<ConfirmDepositPaymentCommand, bool>
{
    public async Task<bool> Handle(ConfirmDepositPaymentCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra Contract
        var contract = await contractRepo.GetByIdAsync(request.ContractId, cancellationToken)
            ?? throw new KeyNotFoundException($"Không tìm thấy hợp đồng với ID: {request.ContractId}");

        if (contract.Status != ContractStatus.AwaitingPayment)
            throw new InvalidOperationException(
                $"Hợp đồng đang ở trạng thái '{contract.Status}', chỉ có thể xác nhận thanh toán khi ở trạng thái AwaitingPayment.");

        await uow.BeginTransactionAsync(cancellationToken);

        try
        {
            var application = contract.Application;
            var newStore = new Domain.Entities.Store
            {
                Id = Guid.NewGuid(),
                Code = $"STR-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
                Name = application.StoreName,
                Address = application.StoreAddress,
                Phone = application.Phone,
                Email = application.Email,
                Type = (int)StoreType.Franchise,
                Status = (int)StoreStatus.ComingSoon, // Đợi thi công xong mới Open
                ImageUrl = application.StoreFrontUrl,
                Latitude = application.StoreLatitude,
                Longitude = application.StoreLongitude,
            };

            contract.DepositStatus = DepositStatus.Paid;
            contract.Status = ContractStatus.Active;
            contract.Application.StoreId = newStore.Id;
            contract.UpdatedAt = DateTimeOffset.UtcNow;

            await storeRepo.AddOnlyAsync(newStore);

            await identityServiceClient.UpdateUserAsync(new UserInfo
            {
                Id = application.ManagerId,
                RoleSlug = "manager",
                HomeStoreId = newStore.Id
            }, cancellationToken);

            await uow.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Contract {ContractId} is now ACTIVE. Store {StoreId} created and User {UserId} promoted to Manager.",
                contract.Id, newStore.Id, application.ManagerId);

            return true;
        }
        catch (Exception ex)
        {
            await uow.RollbackAsync(cancellationToken);

            logger.LogError(ex, "Lỗi khi xác nhận thanh toán cho hợp đồng {ContractId}.", request.ContractId);
            throw;
        }
    }
}