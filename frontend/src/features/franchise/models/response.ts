import {
    FranchiseApplicationStatus,
    DepositStatus,
    ContractSetupStatus,
    ContractStatus
} from "@/features/franchise/constants"

export interface FranchiseContractResponse {
    id: string
    applicationId: string
    docuSealSubmissionId: number
    contractPdfUrl: string
    depositAmount: number
    depositStatus: DepositStatus
    franchiseFee: number
    revenueSharePercentage: number
    status: ContractStatus
    setupStatus: ContractSetupStatus
    validFrom?: string
    validUntil?: string
    createdAt: string
    updatedAt: string
}

export interface FranchiseApplicationResponse {
    id: string
    storeId: string
    managerId: string
    fullName: string
    email: string
    phone: string
    identityNumber: string
    provinceId: string
    districtId: string
    wardId: string
    storeName: string
    storeAddress: string
    storeLatitude: number
    storeLongitude: number
    storeSizeSqm: number
    landCertificateUrls: string[]
    storeFrontUrl: string
    status: FranchiseApplicationStatus
    adminNote: string
    createdAt: string
    updatedAt: string
    reviewedBy?: string
    reviewedAt?: string
    contracts?: FranchiseContractResponse[]
}

export interface FranchiseApplicationDetailResponse {
    application: FranchiseApplicationResponse
    contracts: FranchiseContractResponse[]
}

export interface ReviewApplicationResponse {
    message: string
    contractId?: string
    docuSealSubmissionId?: number
}
