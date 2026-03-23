using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using FranchiseService.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FranchiseService.Application.Features.FranchiseContracts.Commands.UpdateContractStatus;

public class UpdateContractStatusCommand : IRequest<bool>
{
    public Guid ContractId { get; set; }
    public ContractStatus Status { get; set; }
    public string? Reason { get; set; }
}

public class UpdateContractStatusHandler : IRequestHandler<UpdateContractStatusCommand, bool>
{
    private readonly IFranchiseContractRepository _contractRepo;
    private readonly ILogger<UpdateContractStatusHandler> _logger;

    public UpdateContractStatusHandler(
        IFranchiseContractRepository contractRepo,
        ILogger<UpdateContractStatusHandler> logger)
    {
        _contractRepo = contractRepo;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateContractStatusCommand request, CancellationToken cancellationToken)
    {
        var contract = await _contractRepo.GetByIdAsync(request.ContractId)
            ?? throw new KeyNotFoundException($"Không tìm thấy hợp đồng với ID: {request.ContractId}");

        // Kiểm tra logic trạng thái
        if (contract.Status == ContractStatus.Terminated || contract.Status == ContractStatus.Expired)
        {
            throw new InvalidOperationException($"Không thể thay đổi trạng thái của hợp đồng đã ở trạng thái kết thúc ({contract.Status}).");
        }

        // - Chỉ hợp đồng đang Active mới được Tạm ngưng (Suspended)
        if (request.Status == ContractStatus.Suspended && contract.Status != ContractStatus.Active)
        {
            throw new InvalidOperationException("Chỉ có thể tạm ngưng hợp đồng đang ở trạng thái Active.");
        }

        // - Chỉ hợp đồng đang Suspended mới được Mở lại (Active) (Khôi phục sau khi tạm ngưng)
        // (Nếu chuyển từ AwaitingPayment lên Active thì phải dùng ConfirmDepositPayment)
        if (request.Status == ContractStatus.Active && contract.Status != ContractStatus.Suspended)
        {
            throw new InvalidOperationException("Hành động này chỉ dùng để mở khóa hợp đồng đang bị Suspended. Để kích hoạt lần đầu, vui lòng dùng Xác nhận cọc.");
        }

        // - Chấm dứt (Terminated) có thể thực hiện từ Active hoặc Suspended
        if (request.Status == ContractStatus.Terminated && 
            contract.Status != ContractStatus.Active && contract.Status != ContractStatus.Suspended)
        {
            throw new InvalidOperationException("Chỉ có thể chấm dứt hợp đồng khi nó đang Active hoặc Suspended.");
        }

        // Cập nhật trạng thái
        contract.Status = request.Status;
        contract.UpdatedAt = DateTimeOffset.UtcNow;
        
        // TODO: Có thể lưu Reason lịch sử vào bảng ContractHistory nếu có

        await _contractRepo.UpdateAsync(contract);

        _logger.LogInformation(
            "Contract {ContractId} status updated to {Status}. Reason: {Reason}",
            contract.Id, request.Status, request.Reason);

        return true;
    }
}
