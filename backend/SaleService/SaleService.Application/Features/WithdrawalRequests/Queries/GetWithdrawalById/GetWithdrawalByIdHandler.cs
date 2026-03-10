using MediatR;
using SalesService.Application.Dtos.WithdrawalRequests;
using SalesService.Application.Interfaces;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Persisitences.WithdrawalRequest;

namespace SalesService.Application.Features.WithdrawalRequests.Queries.GetWithdrawalById;

public class GetWithdrawalByIdHandler(
    IWithdrawalRequestRepository repo,
    IAesEncryptionService aesService,
    IIdentityServiceClient identityClient
) : IRequestHandler<GetWithdrawalByIdQuery, WithdrawalDetailDto?>
{
    public async Task<WithdrawalDetailDto?> Handle(GetWithdrawalByIdQuery request, CancellationToken ct)
    {
        var item = await repo.GetByIdAsync(request.Id, ct);
        if (item == null) return null;

        // Giải mã số tài khoản từ AES
        string rawAcc;
        try
        {
            rawAcc = aesService.Decrypt(item.EncryptedAccountNumber);
        }
        catch
        {
            rawAcc = item.MaskedAccountNumber;
        }

        string? processedByName = null;
        if (item.ProcessedBy.HasValue)
        {
            try
            {
                var user = await identityClient.GetUserByIdAsync(item.ProcessedBy.Value);
                processedByName = user?.Username ?? user?.Email;
            }
            catch { /* Log error but don't crash */ }
        }

        return new WithdrawalDetailDto
        {
            // Fields của Summary (Cha)
            Id = item.Id,
            RequestCode = item.RequestCode,
            UserId = item.UserId,
            Amount = item.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
            FeeAmount = item.FeeAmount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
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
            UpdatedAt = item.UpdatedAt,

            BankTransactionId = item.BankTransactionId,
            CompletedAt = item.CompletedAt,
            ProcessedById = item.ProcessedBy,
            ProcessedByName = processedByName,
        };
    }
}