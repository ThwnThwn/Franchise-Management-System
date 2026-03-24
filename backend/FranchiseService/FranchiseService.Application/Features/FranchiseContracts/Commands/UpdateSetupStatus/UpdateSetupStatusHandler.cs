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

public class UpdateSetupStatusHandler : IRequestHandler<UpdateSetupStatusCommand, bool>
{
    private readonly IFranchiseContractRepository _contractRepo;
    private readonly ILogger<UpdateSetupStatusHandler> _logger;

    public UpdateSetupStatusHandler(
        IFranchiseContractRepository contractRepo,
        ILogger<UpdateSetupStatusHandler> logger)
    {
        _contractRepo = contractRepo;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateSetupStatusCommand request, CancellationToken cancellationToken)
    {
        var contract = await _contractRepo.GetByIdAsync(request.ContractId)
            ?? throw new KeyNotFoundException($"Không tìm thấy hợp đồng với ID: {request.ContractId}");

        if (contract.Status != ContractStatus.Active)
            throw new InvalidOperationException(
                "Chỉ có thể cập nhật tiến độ thi công khi hợp đồng đang ở trạng thái Active.");

        contract.SetupStatus = request.SetupStatus;
        contract.UpdatedAt = DateTimeOffset.UtcNow;
        await _contractRepo.UpdateAsync(contract);

        _logger.LogInformation(
            "Contract {ContractId} setup status updated to {Status} by {UserId}",
            contract.Id, request.SetupStatus, request.UpdatedBy);

        return true;
    }
}
