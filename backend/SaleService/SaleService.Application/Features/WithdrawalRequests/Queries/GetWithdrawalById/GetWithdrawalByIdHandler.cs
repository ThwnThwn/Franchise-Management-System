using MediatR;
using SalesService.Application.Dtos.WithdrawalRequests;
using SalesService.Application.Interfaces;
using SalesService.Application.Persisitences.WithdrawalRequest;

namespace SalesService.Application.Features.WithdrawalRequests.Queries.GetWithdrawalById;

public class GetWithdrawalByIdHandler(IWithdrawalRequestRepository repo, IAesEncryptionService aesService)
    : IRequestHandler<GetWithdrawalByIdQuery, WithdrawalDetailDto?>
{
    private readonly IWithdrawalRequestRepository _repo = repo;
    private readonly IAesEncryptionService _aesService = aesService;

    public async Task<WithdrawalDetailDto?> Handle(GetWithdrawalByIdQuery request, CancellationToken ct)
    {
        var item = await _repo.GetByIdAsync(request.Id, ct);
        if (item == null) return null;

        // Giải mã số tài khoản từ AES
        var rawAcc = _aesService.Decrypt(item.EncryptedAccountNumber);

        return new WithdrawalDetailDto
        {
            // Fields của Summary (Cha)
            Id = item.Id,
            RequestCode = item.RequestCode,
            UserId = item.UserId,
            Amount = item.Amount,
            FeeAmount = item.FeeAmount,
            BankCode = item.BankCode,
            BankName = item.BankName,
            AccountName = item.AccountName,
            MaskedAccountNumber = item.MaskedAccountNumber,
            Status = item.Status,
            CreatedAt = item.CreatedAt,

            // Fields của Detail (Con)
            RawAccountNumber = rawAcc,
            Notes = item.Notes,
            InternalNotes = item.InternalNotes,
            ApprovalNotes = item.ApprovalNotes,
            RejectionReason = item.RejectionReason,
            CancellationReason = item.CancellationReason,
            ReceiptImageUrl = item.ReceiptImageUrl,
            TransactionId = item.TransactionId,
            UpdatedAt = item.UpdatedAt
        };
    }
}