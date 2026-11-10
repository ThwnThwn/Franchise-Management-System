import {
    FranchiseApplicationStatus,
    DepositStatus,
    ContractSetupStatus,
    ContractStatus
} from "@/features/franchise/constants"

// ============================================================================
// MAPPING TEXT HIỂN THỊ (UI LABELS)
// ============================================================================

export const FRANCHISE_APPLICATION_STATUS_LABELS: Record<FranchiseApplicationStatus, string> = {
    [FranchiseApplicationStatus.Pending]: "Chờ thẩm định",
    [FranchiseApplicationStatus.Approved]: "Đã duyệt",
    [FranchiseApplicationStatus.Rejected]: "Từ chối"
}

export const DEPOSIT_STATUS_LABELS: Record<DepositStatus, string> = {
    [DepositStatus.AwaitingPayment]: "Chờ thanh toán",
    [DepositStatus.Paid]: "Đã thanh toán"
}

export const CONTRACT_SETUP_STATUS_LABELS: Record<ContractSetupStatus, string> = {
    [ContractSetupStatus.Design]: "Đang thiết kế",
    [ContractSetupStatus.Construction]: "Đang thi công",
    [ContractSetupStatus.Inspection]: "Đang nghiệm thu",
    [ContractSetupStatus.Ready]: "Sẵn sàng"
}

export const CONTRACT_STATUS_LABELS: Record<ContractStatus, string> = {
    [ContractStatus.Draft]: "Bản nháp",
    [ContractStatus.Approved]: "Đã duyệt",
    [ContractStatus.AwaitingSignature]: "Chờ ký (DocuSeal)",
    [ContractStatus.AwaitingPayment]: "Chờ đóng cọc",
    [ContractStatus.Active]: "Đang hoạt động",
    [ContractStatus.Suspended]: "Tạm ngưng",
    [ContractStatus.Terminated]: "Đã chấm dứt",
    [ContractStatus.Expired]: "Hết hạn"
}
