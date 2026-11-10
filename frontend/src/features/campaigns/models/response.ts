import { Voucher } from "@/features/vouchers/models/response"

export interface CampaignResponse {
    id: string
    name: string
    description: string
    startDate?: string // ISO string
    endDate?: string // ISO string
    createdAt?: string // ISO string
    updatedAt?: string // ISO string
    vouchers: Voucher[]
    isActive: boolean
}

export interface UpdateActiveStatusRequest {
    isActive: boolean
}
