using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using FranchiseService.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FranchiseService.Application.Features.FranchiseContracts.Commands.RenewContract;

public class RenewContractCommand : IRequest<bool>
{
    public Guid ContractId { get; set; }
    public int ExtensionYears { get; set; }
    public string? Note { get; set; }
}

public class RenewContractHandler : IRequestHandler<RenewContractCommand, bool>
{
    private readonly IFranchiseContractRepository _contractRepo;
    private readonly ILogger<RenewContractHandler> _logger;

    public RenewContractHandler(
        IFranchiseContractRepository contractRepo,
        ILogger<RenewContractHandler> logger)
    {
        _contractRepo = contractRepo;
        _logger = logger;
    }

    public async Task<bool> Handle(RenewContractCommand request, CancellationToken cancellationToken)
    {
        if (request.ExtensionYears <= 0)
        {
            throw new ArgumentException("Số năm gia hạn phải lớn hơn 0.");
        }

        var contract = await _contractRepo.GetByIdAsync(request.ContractId)
            ?? throw new KeyNotFoundException($"Không tìm thấy hợp đồng với ID: {request.ContractId}");

        if (contract.Status == ContractStatus.Terminated)
        {
            throw new InvalidOperationException($"Không thể gia hạn hợp đồng đã bị chấm dứt ({contract.Status}).");
        }
        
        // Gia hạn ngày hết hạn
        if (!contract.ValidUntil.HasValue) 
        {
            throw new InvalidOperationException("Hợp đồng hiện tại không có ngày hết hạn hợp lệ.");
        }

        var oldValidUntil = contract.ValidUntil.Value;
        contract.ValidUntil = contract.ValidUntil.Value.AddYears(request.ExtensionYears);
        contract.UpdatedAt = DateTimeOffset.UtcNow;
        
        // Nếu hợp đồng đang Expired thì cho Active trở lại
        if (contract.Status == ContractStatus.Expired)
        {
            contract.Status = ContractStatus.Active;
        }

        await _contractRepo.UpdateAsync(contract);

        _logger.LogInformation(
            "Contract {ContractId} renewed for {Years} years. ValidUntil extended from {OldDate} to {NewDate}. Note: {Note}",
            contract.Id, request.ExtensionYears, oldValidUntil, contract.ValidUntil, request.Note);

        return true;
    }
}
