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

public class ConfirmDepositPaymentHandler : IRequestHandler<ConfirmDepositPaymentCommand, bool>
{
    private readonly IFranchiseContractRepository _contractRepo;
    private readonly ILogger<ConfirmDepositPaymentHandler> _logger;

    public ConfirmDepositPaymentHandler(
        IFranchiseContractRepository contractRepo,
        ILogger<ConfirmDepositPaymentHandler> logger)
    {
        _contractRepo = contractRepo;
        _logger = logger;
    }

    public async Task<bool> Handle(ConfirmDepositPaymentCommand request, CancellationToken cancellationToken)
    {
        var contract = await _contractRepo.GetByIdAsync(request.ContractId)
            ?? throw new KeyNotFoundException($"Không tìm thấy hợp đồng với ID: {request.ContractId}");

        if (contract.Status != ContractStatus.AwaitingPayment)
            throw new InvalidOperationException(
                $"Hợp đồng đang ở trạng thái '{contract.Status}', chỉ có thể xác nhận thanh toán khi ở trạng thái AwaitingPayment.");

        // Xác nhận đã thanh toán → hợp đồng có hiệu lực
        contract.DepositStatus = DepositStatus.Paid;
        contract.Status = ContractStatus.Active;
        contract.UpdatedAt = DateTimeOffset.UtcNow;

        await _contractRepo.UpdateAsync(contract);

        _logger.LogInformation(
            "Contract {ContractId} is now ACTIVE after deposit confirmed by {UserId}",
            contract.Id, request.ConfirmedBy);

        // TODO Phase 3: Publish event ContractActivated → Kafka → Tạo Store Record

        return true;
    }
}
