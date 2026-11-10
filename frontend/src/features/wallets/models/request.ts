import { PaginationParams } from "@/models/common/request"
import { WalletStatus, WithdrawalStatus } from "@/features/wallets/constants"

export interface TopUpWalletRequest {
    amount: number
    returnUrl?: string
}

export interface GetAllWalletTransactionsRequest extends PaginationParams {}

export interface UpdateWalletStatusRequest {
    newStatus: WalletStatus
    reason: string
    adminNotes: string
}

// --- NHÓM END-USER ---

export interface CreateWithdrawalReq {
    amount: string
    bankCode?: string
    bankName: string
    accountName: string
    accountNumber: string
    notes?: string
}

export interface GetMyWithdrawalsReq extends PaginationParams {
    status?: WithdrawalStatus
}

export interface CancelMyWithdrawalReq {
    cancellationReason: string
}

// --- NHÓM ADMIN ---

export interface GetAllWithdrawalsReq extends PaginationParams {
    userId?: string
    status?: WithdrawalStatus
}

export interface GetWithdrawalByIdReq {
    requestId: string
}

export interface ApproveWithdrawalReq {
    approvalNotes?: string
}

export interface RejectWithdrawalReq {
    rejectionReason: string
}

export interface CancelWithdrawalReq {
    cancellationReason: string
}

/** * Note: Interface này dùng để định nghĩa dữ liệu cho FormData
 * khi gửi kèm file ReceiptFile
 */
export interface CompleteWithdrawalReq {
    bankTransactionId: string
    receiptFile?: File // []byte bên Go, xử lý MultipartForm
}
