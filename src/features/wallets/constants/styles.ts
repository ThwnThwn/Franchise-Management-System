import { ChipProps } from "@heroui/react"
import * as Enums from "./enums"

// ==========================================
// WALLET STYLES
// ==========================================

export const WalletStatusColors: Record<Enums.WalletStatus, ChipProps["color"]> = {
    [Enums.WalletStatus.Inactive]: "default",
    [Enums.WalletStatus.Active]: "success",
    [Enums.WalletStatus.Locked]: "danger",
    [Enums.WalletStatus.Suspended]: "warning"
}

export const TransactionTypeColors: Record<Enums.TransactionType, ChipProps["color"]> = {
    [Enums.TransactionType.Deposit]: "primary",
    [Enums.TransactionType.Payment]: "secondary",
    [Enums.TransactionType.Refund]: "success",
    [Enums.TransactionType.Adjustment]: "default",
    [Enums.TransactionType.Withdrawal]: "warning",
    [Enums.TransactionType.Transfer]: "primary"
}

export const TransactionStatusColors: Record<Enums.TransactionStatus, ChipProps["color"]> = {
    [Enums.TransactionStatus.Pending]: "warning",
    [Enums.TransactionStatus.Completed]: "success",
    [Enums.TransactionStatus.Failed]: "danger",
    [Enums.TransactionStatus.Cancelled]: "default"
}

export const WalletReferenceTypeColors: Record<Enums.WalletReferenceType, ChipProps["color"]> = {
    [Enums.WalletReferenceType.Order]: "primary",
    [Enums.WalletReferenceType.RefundTicket]: "success",
    [Enums.WalletReferenceType.InternalUser]: "secondary",
    [Enums.WalletReferenceType.System]: "default"
}

export const WalletTransactionPaymentMethodColors: Record<
    Enums.WalletTransactionPaymentMethod,
    ChipProps["color"]
> = {
    [Enums.WalletTransactionPaymentMethod.Cash]: "default",
    [Enums.WalletTransactionPaymentMethod.Vnpay]: "primary",
    [Enums.WalletTransactionPaymentMethod.Wallet]: "secondary"
}

// ==========================================
// REFUND STYLES
// ==========================================

export const RefundTicketStatusColors: Record<Enums.RefundTicketStatus, ChipProps["color"]> = {
    [Enums.RefundTicketStatus.Pending]: "warning",
    [Enums.RefundTicketStatus.Approved]: "primary",
    [Enums.RefundTicketStatus.Processing]: "warning",
    [Enums.RefundTicketStatus.Completed]: "success",
    [Enums.RefundTicketStatus.Failed]: "danger",
    [Enums.RefundTicketStatus.Rejected]: "danger",
    [Enums.RefundTicketStatus.Cancelled]: "default"
}

export const RefundReasonTypeColors: Record<Enums.RefundReasonType, ChipProps["color"]> = {
    [Enums.RefundReasonType.NormalProcess]: "default",
    [Enums.RefundReasonType.SystemError]: "danger",
    [Enums.RefundReasonType.CustomerComplaint]: "warning",
    [Enums.RefundReasonType.GoodwillGesture]: "success",
    [Enums.RefundReasonType.Compensation]: "secondary"
}

export const RefundTicketMethodColors: Record<Enums.RefundTicketMethod, ChipProps["color"]> = {
    [Enums.RefundTicketMethod.Transfer]: "primary",
    [Enums.RefundTicketMethod.Cash]: "default"
}

// ==========================================
// WITHDRAWAL STYLES
// ==========================================

export const WithdrawalStatusColors: Record<Enums.WithdrawalStatus, ChipProps["color"]> = {
    [Enums.WithdrawalStatus.Pending]: "warning",
    [Enums.WithdrawalStatus.Approved]: "primary",
    [Enums.WithdrawalStatus.Processing]: "warning",
    [Enums.WithdrawalStatus.Completed]: "success",
    [Enums.WithdrawalStatus.Failed]: "danger",
    [Enums.WithdrawalStatus.Rejected]: "danger",
    [Enums.WithdrawalStatus.Cancelled]: "default"
}
