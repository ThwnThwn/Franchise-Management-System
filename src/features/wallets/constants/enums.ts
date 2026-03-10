// ==========================================
// WALLET ENUMS
// ==========================================

export enum WalletStatus {
    Inactive = 1,
    Active = 2,
    Locked = 3,
    Suspended = 4
}

export enum TransactionType {
    Deposit = 1,
    Payment = 2,
    Refund = 3,
    Adjustment = 4,
    Withdrawal = 5,
    Transfer = 6
}

export enum TransactionStatus {
    Pending = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}

export enum WalletReferenceType {
    Order = 1,
    RefundTicket = 2,
    InternalUser = 3,
    System = 4
}

export enum WalletTransactionPaymentMethod {
    Cash = 1,
    Vnpay = 2,
    Wallet = 3
}

// ==========================================
// REFUND ENUMS
// ==========================================

export enum RefundTicketStatus {
    Pending = 1,
    Approved = 2,
    Processing = 3,
    Completed = 4,
    Failed = 5,
    Rejected = 6,
    Cancelled = 7
}

export enum RefundReasonType {
    NormalProcess = 1,
    SystemError = 2,
    CustomerComplaint = 3,
    GoodwillGesture = 4,
    Compensation = 5
}

export enum RefundTicketMethod {
    Transfer = 1,
    Cash = 2
}

// ==========================================
// WITHDRAWAL ENUMS
// ==========================================

export enum WithdrawalStatus {
    Pending = 1, // Chờ duyệt
    Approved = 2, // Đã duyệt
    Processing = 3, // Đang xử lý
    Completed = 4, // Thành công
    Failed = 5, // Thất bại
    Rejected = 6, // Từ chối
    Cancelled = 7 // Đã hủy
}
