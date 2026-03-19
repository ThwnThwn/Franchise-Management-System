import axiosInstance from "@/utils/axios"
import {
    CreateVoucherRequest,
    UpdateVoucherRequest,
    UpdateVoucherStatusRequest
} from "../models/request"
import { Voucher } from "../models/response"

export const voucherApi = {
    getAll: async (): Promise<Voucher[]> => {
        const res = await axiosInstance.get("/vouchers")
        return res.data
    },
    create: async (req: CreateVoucherRequest): Promise<Voucher> => {
        const res = await axiosInstance.post("/vouchers", req)
        return res.data
    },
    update: async (id: string, req: UpdateVoucherRequest): Promise<void> => {
        await axiosInstance.put(`/vouchers/${id}`, req)
    },
    updateStatus: async (id: string, req: UpdateVoucherStatusRequest): Promise<void> => {
        await axiosInstance.patch(`/vouchers/${id}/status`, req)
    },
    delete: async (id: string): Promise<void> => {
        await axiosInstance.delete(`/vouchers/${id}`)
    },
    getMyVouchers: async (): Promise<Voucher[]> => {
        const res = await axiosInstance.get("/me/vouchers")
        return res.data
    },
    redeemVoucher: async (id: string): Promise<void> => {
        await axiosInstance.post(`/me/vouchers/${id}/redeem`)
    },
    collectVoucher: async (id: string): Promise<void> => {
        await axiosInstance.post(`/me/vouchers/${id}/collect`)
    },
    assignVoucher: async (id: string, userIds: string[]): Promise<void> => {
        await axiosInstance.post(`/vouchers/${id}/assign`, { userIds })
    }
}
