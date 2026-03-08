import axiosInstance from "@/utils/axios"
import { ApplyForShiftRequest, ReviewApplicationRequest } from "../models/request"
import { ShiftApplicationItem } from "../models/response"

export const shiftApplicationApi = {
    apply: async ({
        shiftId,
        req
    }: {
        shiftId: string
        req: ApplyForShiftRequest
    }): Promise<{ message?: string }> => {
        const res = await axiosInstance.post(`/shifts/${shiftId}/applications`, req)
        return res.data
    },

    getByShiftId: async ({ shiftId }: { shiftId: string }): Promise<ShiftApplicationItem[]> => {
        const res = await axiosInstance.get(`/shifts/${shiftId}/applications`)
        return res.data
    },

    review: async ({
        id,
        req
    }: {
        id: string
        req: ReviewApplicationRequest
    }): Promise<{ message?: string }> => {
        const res = await axiosInstance.patch(`/shift-applications/${id}/review`, req)
        return res.data
    }
}
