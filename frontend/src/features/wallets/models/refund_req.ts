import { PaginationParams } from "@/models/common/request"
import { RefundTicketStatus } from "@/features/wallets/constants"

export interface GetAllRefundsRequest extends PaginationParams {
    status?: RefundTicketStatus // *enum
}

export interface CreateRefundByCustomerRequest {
    amount: string
    reason: string
}

export interface ApproveRefundRequest {
    approvalNotes: string
}

export interface RejectRefundRequest {
    rejectNotes: string
}

export interface CancelRefundRequest {
    cancellationReason: string
}

export interface SetCompleteRefundRequest {
    completeNotes?: string // *string
}
