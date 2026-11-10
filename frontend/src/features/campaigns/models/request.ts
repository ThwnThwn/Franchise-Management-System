export interface CreateCampaignRequest {
    name: string
    description: string
    startDate: string // ISO string
    endDate: string // ISO string
    voucherIds: string[]
}
