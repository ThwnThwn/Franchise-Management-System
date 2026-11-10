import { ChipProps } from "@heroui/react"
import {
    FranchiseApplicationStatus,
    DepositStatus,
    ContractSetupStatus,
    ContractStatus
} from "@/features/franchise/constants"

// ============================================================================
// MAPPING MÀU SẮC (HEROUI CHIP COLORS)
// ============================================================================

export const FRANCHISE_APPLICATION_STATUS_COLORS: Record<
    FranchiseApplicationStatus,
    ChipProps["color"]
> = {
    [FranchiseApplicationStatus.Pending]: "warning",
    [FranchiseApplicationStatus.Approved]: "success",
    [FranchiseApplicationStatus.Rejected]: "danger"
}

export const DEPOSIT_STATUS_COLORS: Record<DepositStatus, ChipProps["color"]> = {
    [DepositStatus.AwaitingPayment]: "warning",
    [DepositStatus.Paid]: "success"
}

export const CONTRACT_SETUP_STATUS_COLORS: Record<ContractSetupStatus, ChipProps["color"]> = {
    [ContractSetupStatus.Design]: "secondary",
    [ContractSetupStatus.Construction]: "warning",
    [ContractSetupStatus.Inspection]: "primary",
    [ContractSetupStatus.Ready]: "success"
}

export const CONTRACT_STATUS_COLORS: Record<ContractStatus, ChipProps["color"]> = {
    [ContractStatus.Draft]: "default",
    [ContractStatus.Approved]: "secondary",
    [ContractStatus.AwaitingSignature]: "primary",
    [ContractStatus.AwaitingPayment]: "warning",
    [ContractStatus.Active]: "success",
    [ContractStatus.Suspended]: "danger",
    [ContractStatus.Terminated]: "danger",
    [ContractStatus.Expired]: "default"
}
