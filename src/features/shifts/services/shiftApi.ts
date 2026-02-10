import { buildQueryParams, requestWrapper } from "@/utils/helpers/axiosHelper"
import axiosInstance from "@/utils/axios"
import { PageResult } from "@/models/common/respone"
import { CreateShiftRequest, GetShiftsParams, UpdateShiftRequest } from "../models/request"
import { ShiftResponse } from "../models/response"

export const shiftApi = {
    getAll: ({ query }: { query: GetShiftsParams }) =>
        requestWrapper<PageResult<ShiftResponse>>(async () => {
            const params = {
                ...buildQueryParams(query)
                // ...buildQueryParams(pagination)
            }
            const res = await axiosInstance.get("/shifts", { params })
            return res.data
        }),

    getById: ({ id }: { id: string }) =>
        requestWrapper<ShiftResponse>(async () => {
            const res = await axiosInstance.get(`/shifts/${id}`)
            return res.data
        }),

    create: (req: CreateShiftRequest) =>
        requestWrapper<void>(async () => {
            await axiosInstance.post("/shifts", req)
        }),

    update: ({ id, req }: { id: string; req: UpdateShiftRequest }) =>
        requestWrapper<void>(async () => {
            await axiosInstance.patch(`/shifts/${id}`, req)
        }),

    delete: ({ id }: { id: string }) =>
        requestWrapper<void>(async () => {
            await axiosInstance.delete(`/shifts/${id}`)
        }),

    // Bonus: API Publish nhanh (Thay đổi status -> Published)
    publish: ({ id }: { id: string }) =>
        requestWrapper<void>(async () => {
            await axiosInstance.post(`/shifts/${id}/publish`)
        }),

    assignStaff: ({ id, userIds }: { id: string; userIds: string[] }) =>
        requestWrapper<void>(async () => {
            await axiosInstance.post(`/shifts/${id}/assign`, { userIds })
        }),

    removeStaff: ({ id, userId }: { id: string; userId: string }) =>
        requestWrapper<void>(async () => {
            await axiosInstance.delete(`/shifts/${id}/assign/${userId}`)
        })
}
