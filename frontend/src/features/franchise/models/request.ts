import {
    FranchiseApplicationStatus,
    ContractSetupStatus,
    ContractStatus
} from "@/features/franchise/constants"
import { PaginationParams } from "@/models/common/request"

// --- APPLICATIONS ---

export interface SubmitApplicationRequest {
    fullName: string
    email: string
    phone: string
    identityNumber: string
    provinceId: string
    districtId: string
    wardId: string
    storeName: string
    storeAddress: string
    storeLatitude?: number
    storeLongitude?: number
    storeSizeSqm?: number

    // Dành cho Frontend xử lý File (sẽ append vào FormData)
    storeFront?: File
    landCerts?: File[]
}

export interface GetAllApplicationsRequest extends PaginationParams {
    status?: FranchiseApplicationStatus
    searchKey?: string
}

export interface ReviewFranchiseApplicationRequest {
    action: number // 1: Approve, 2: Reject
    note?: string
    depositAmount?: number
    franchiseFee?: number
    revenueSharePercentage?: number
    validFrom?: string // ISO string Date
    validUntil?: string // ISO string Date
}

// --- CONTRACTS ---

export interface GetAllContractsRequest extends PaginationParams {
    status?: ContractStatus
}

export interface ConfirmDepositPaymentRequest {
    note?: string
}

export interface UpdateSetupStatusRequest {
    setupStatus: ContractSetupStatus
}

export interface UpdateContractStatusRequest {
    status: ContractStatus
    reason: string
}

export interface RenewContractRequest {
    extensionYears: number
    note?: string
}
