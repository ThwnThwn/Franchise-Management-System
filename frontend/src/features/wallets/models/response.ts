import {
    WalletStatus,
    TransactionType,
    TransactionStatus,
    WalletReferenceType,
    WalletTransactionPaymentMethod,
    WithdrawalStatus
} from "@/features/wallets/constants"

export interface WalletResponse {
    id: string
    userId: string
    balance: string
    status: WalletStatus
    createdAt: string
    updatedAt: string
}

export interface TopUpVnPayReturnResponse {
    success: boolean
    walletTransactionId: string
    amount: number
    newBalance: number
}

export interface WalletTransactionResponse {
    id: string
    walletId: string
    userId: string
    orderId: string
    initiatedBy: string
    processedBy: string
    parentTransactionId: string
    transactionCode: string
    notes: string
    amount: string
    balanceBefore: string
    balanceAfter: string
    transactionType: TransactionType
    status: TransactionStatus
    referenceId: string
    referenceType: WalletReferenceType
    description: string
    externalTransactionId: string
    metadata: string
    paymentMethod: WalletTransactionPaymentMethod
    createdAt: string
    updatedAt: string
    completedAt: string
    failedAt: string
    processedAt: string
    failureReason: string
}

export interface WithdrawalBaseRes {
    id: string
    message: string
}

export interface WithdrawalSummary {
    id: string
    requestCode: string
    userId: string
    amount: string
    feeAmount: string
    bankCode?: string // Tương ứng *string trong Go
    bankName: string
    accountName: string
    maskedAccountNumber: string
    status: WithdrawalStatus
    createdAt: string
}

export interface WithdrawalDetailRes extends WithdrawalSummary {
    rawAccountNumber: string
    notes: string
    internalNotes: string
    approvalNotes: string
    rejectionReason: string
    cancellationReason: string
    receiptImageUrl: string
    transactionId: string
    updatedAt: string

    // Các field Audit Trail (nullable)
    processedById?: string
    processedByName?: string
    bankTransactionId?: string
    completedAt?: string
}
