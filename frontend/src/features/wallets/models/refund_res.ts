import {
    RefundTicketStatus,
    RefundReasonType,
    RefundTicketMethod
} from "@/features/wallets/constants"

export interface RefundTicketSummary {
    id: string
    ticketCode: string
    amount: string
    refundAmount: string
    reason: string
    userId: string
    transactionId: string
    status: RefundTicketStatus
    createdAt: string
    updatedAt: string
}

export interface RefundTicketDetail extends RefundTicketSummary {
    rejectionReason: string
    walletId: string
    originalTransactionId: string
    refundReason: RefundReasonType
    refundMethod: RefundTicketMethod
}
