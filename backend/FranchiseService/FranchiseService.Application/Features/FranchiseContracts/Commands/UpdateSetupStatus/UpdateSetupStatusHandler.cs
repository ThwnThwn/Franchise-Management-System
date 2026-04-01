using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FranchiseService.Application.Features.FranchiseContracts.Commands.UpdateSetupStatus;

/// <summary>
/// Command: Cập nhật tiến độ thi công / setup cửa hàng.
/// Design → Construction → Inspection → Ready
/// </summary>
public class UpdateSetupStatusCommand : IRequest<bool>
{
    public Guid ContractId { get; set; }
    public ContractSetupStatus SetupStatus { get; set; }
    public Guid UpdatedBy { get; set; }
}

public class UpdateSetupStatusHandler(
    IFranchiseUow uow,
    IFranchiseContractRepository contractRepo,
    IStoreRepository storeRepo,
    IIdentityServiceClient identityServiceClient,
    ILogger<UpdateSetupStatusHandler> logger
) : IRequestHandler<UpdateSetupStatusCommand, bool>
{
    public async Task<bool> Handle(UpdateSetupStatusCommand request, CancellationToken cancellationToken)
    {
        await uow.BeginTransactionAsync(cancellationToken);

        try
        {
            var contract = await contractRepo.GetByIdAsync(request.ContractId)
                ?? throw new KeyNotFoundException($"Không tìm thấy hợp đồng với ID: {request.ContractId}");

            if (contract.Status != ContractStatus.Active)
                throw new InvalidOperationException(
                    "Chỉ có thể cập nhật tiến độ thi công khi hợp đồng đang ở trạng thái Active.");

            contract.SetupStatus = request.SetupStatus;
            contract.UpdatedAt = DateTimeOffset.UtcNow;

            if (request.SetupStatus == ContractSetupStatus.Ready)
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
                    Status = (int)StoreStatus.Open,
                    ImageUrl = application.StoreFrontUrl,
                    Latitude = application.StoreLatitude,
                    Longitude = application.StoreLongitude,
                };

                contract.Application.StoreId = newStore.Id;

                await storeRepo.AddOnlyAsync(newStore);

                await identityServiceClient.UpdateUserAsync(new UserInfo
                {
                    Id = application.ManagerId,
                    RoleSlug = "manager",
                    HomeStoreId = newStore.Id
                }, cancellationToken);
            }

            await uow.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Contract {ContractId} setup status updated to {Status} by {UserId}",
                contract.Id, request.SetupStatus, request.UpdatedBy);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating setup status for contract {ContractId}", request.ContractId);
            await uow.RollbackAsync(cancellationToken);
            throw;
        }
    }
}