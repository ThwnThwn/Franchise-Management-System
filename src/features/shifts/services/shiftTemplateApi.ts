import { buildQueryParams, requestWrapper } from "@/utils/helpers/axiosHelper"
import axiosInstance from "@/utils/axios"
import {
    CreateShiftTemplateRequest,
    GetShiftTemplatesParams,
    UpdateShiftTemplateRequest
} from "../models/request"
import { ShiftTemplateResponse } from "../models/response"

export const shiftTemplateApi = {
    getAll: ({
        query
        // pagination
    }: {
        query: GetShiftTemplatesParams
        // pagination?: PaginationParams
    }) =>
        requestWrapper<ShiftTemplateResponse[]>(async () => {
            const params = {
                ...buildQueryParams(query)
                // ...(pagination ? buildQueryParams(pagination) : {})
            }
            const res = await axiosInstance.get("/shift-templates", { params })
            return res.data
        }),

    getById: ({ id }: { id: string }) =>
        requestWrapper<ShiftTemplateResponse>(async () => {
            const res = await axiosInstance.get(`/shift-templates/${id}`)
            return res.data
        }),

    create: (req: CreateShiftTemplateRequest) =>
        requestWrapper<void>(async () => {
            await axiosInstance.post("/shift-templates", req)
        }),

    update: ({ id, req }: { id: string; req: UpdateShiftTemplateRequest }) =>
        requestWrapper<void>(async () => {
            await axiosInstance.patch(`/shift-templates/${id}`, req)
        }),

    delete: ({ id }: { id: string }) =>
        requestWrapper<void>(async () => {
            await axiosInstance.delete(`/shift-templates/${id}`)
        })
}
