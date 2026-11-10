import * as Enums from "./enums"

// ==========================================
// WALLET LABELS
// ==========================================

export const WalletStatusLabels: Record<Enums.WalletStatus, string> = {
    [Enums.WalletStatus.Inactive]: "Chưa kích hoạt",
    [Enums.WalletStatus.Active]: "Đang hoạt động",
    [Enums.WalletStatus.Locked]: "Đã khóa",
    [Enums.WalletStatus.Suspended]: "Tạm đình chỉ"
}

export const TransactionTypeLabels: Record<Enums.TransactionType, string> = {
    [Enums.TransactionType.Deposit]: "Nạp tiền",
    [Enums.TransactionType.Payment]: "Thanh toán",
    [Enums.TransactionType.Refund]: "Hoàn tiền",
    [Enums.TransactionType.Adjustment]: "Điều chỉnh số dư",
    [Enums.TransactionType.Withdrawal]: "Rút tiền",
    [Enums.TransactionType.Transfer]: "Chuyển tiền nội bộ"
}

export const TransactionStatusLabels: Record<Enums.TransactionStatus, string> = {
    [Enums.TransactionStatus.Pending]: "Đang xử lý",
    [Enums.TransactionStatus.Completed]: "Thành công",
    [Enums.TransactionStatus.Failed]: "Thất bại",
    [Enums.TransactionStatus.Cancelled]: "Đã hủy"
}

export const WalletReferenceTypeLabels: Record<Enums.WalletReferenceType, string> = {
    [Enums.WalletReferenceType.Order]: "Đơn hàng",
    [Enums.WalletReferenceType.RefundTicket]: "Phiếu hoàn tiền",
    [Enums.WalletReferenceType.InternalUser]: "Người dùng nội bộ",
    [Enums.WalletReferenceType.System]: "Hệ thống"
}

export const WalletTransactionPaymentMethodLabels: Record<
    Enums.WalletTransactionPaymentMethod,
    string
> = {
    [Enums.WalletTransactionPaymentMethod.Cash]: "Tiền mặt",
    [Enums.WalletTransactionPaymentMethod.Vnpay]: "VNPay",
    [Enums.WalletTransactionPaymentMethod.Wallet]: "Ví điện tử"
}

// ==========================================
// REFUND LABELS
// ==========================================

export const RefundTicketStatusLabels: Record<Enums.RefundTicketStatus, string> = {
    [Enums.RefundTicketStatus.Pending]: "Chờ duyệt",
    [Enums.RefundTicketStatus.Approved]: "Đã duyệt",
    [Enums.RefundTicketStatus.Processing]: "Đang xử lý",
    [Enums.RefundTicketStatus.Completed]: "Hoàn tất",
    [Enums.RefundTicketStatus.Failed]: "Thất bại",
    [Enums.RefundTicketStatus.Rejected]: "Từ chối",
    [Enums.RefundTicketStatus.Cancelled]: "Đã hủy"
}

export const RefundReasonTypeLabels: Record<Enums.RefundReasonType, string> = {
    [Enums.RefundReasonType.NormalProcess]: "Quy trình thông thường",
    [Enums.RefundReasonType.SystemError]: "Lỗi hệ thống",
    [Enums.RefundReasonType.CustomerComplaint]: "Khách hàng khiếu nại",
    [Enums.RefundReasonType.GoodwillGesture]: "Hỗ trợ khách hàng",
    [Enums.RefundReasonType.Compensation]: "Bồi thường"
}

export const RefundTicketMethodLabels: Record<Enums.RefundTicketMethod, string> = {
    [Enums.RefundTicketMethod.Transfer]: "Chuyển khoản ngân hàng",
    [Enums.RefundTicketMethod.Cash]: "Tiền mặt"
}

// ==========================================
// WITHDRAWAL LABELS
// ==========================================

export const WithdrawalStatusLabels: Record<Enums.WithdrawalStatus, string> = {
    [Enums.WithdrawalStatus.Pending]: "Chờ duyệt",
    [Enums.WithdrawalStatus.Approved]: "Đã duyệt",
    [Enums.WithdrawalStatus.Processing]: "Đang xử lý",
    [Enums.WithdrawalStatus.Completed]: "Thành công",
    [Enums.WithdrawalStatus.Failed]: "Thất bại",
    [Enums.WithdrawalStatus.Rejected]: "Từ chối",
    [Enums.WithdrawalStatus.Cancelled]: "Đã hủy"
}
