import axiosInstance from "@/utils/axios"
import { CampaignResponse, UpdateActiveStatusRequest } from "../models/response"
import { CreateCampaignRequest } from "../models/request"

export const campaignApi = {
    getAll: async (): Promise<CampaignResponse[]> => {
        const res = await axiosInstance.get("/campaigns")
        return res.data
    },

    getActive: async (): Promise<CampaignResponse[]> => {
        const res = await axiosInstance.get("/campaigns/active")
        return res.data
    },

    getById: async ({ id }: { id: string }): Promise<CampaignResponse> => {
        const res = await axiosInstance.get(`/campaigns/${id}`)
        return res.data
    },

    create: async (req: CreateCampaignRequest): Promise<{ message?: string }> => {
        const res = await axiosInstance.post("/campaigns", req)
        return res.data
    },

    updateActive: async ({
        id,
        req
    }: {
        id: string
        req: UpdateActiveStatusRequest
    }): Promise<{ message?: string }> => {
        const res = await axiosInstance.patch(`/campaigns/${id}/active`, req)
        return res.data
    },

    redeemVoucher: async (voucherId: string): Promise<void> => {
        await axiosInstance.post(`/campaigns/vouchers/${voucherId}/redeem`)
    }
}
