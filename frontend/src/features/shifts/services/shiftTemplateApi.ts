import { buildQueryParams } from "@/utils/helpers/axiosHelper"
import axiosInstance from "@/utils/axios"
import {
    CreateShiftTemplateRequest,
    GetShiftTemplatesParams,
    UpdateShiftTemplateRequest
} from "../models/request"
import { ShiftTemplateResponse } from "../models/response"

/**
 * SHIFT TEMPLATE API
 * Quản lý các mẫu ca làm việc (Sáng, Chiều, Tối...)
 */
export const shiftTemplateApi = {
    // Lấy toàn bộ danh sách mẫu ca theo Store
    getAll: async ({
        query
    }: {
        query: GetShiftTemplatesParams
    }): Promise<ShiftTemplateResponse[]> => {
        const params = {
            ...buildQueryParams(query)
        }
        const res = await axiosInstance.get("/shift-templates", { params })
        return res.data
    },

    // Lấy chi tiết một mẫu ca
    getById: async ({ id }: { id: string }): Promise<ShiftTemplateResponse> => {
        const res = await axiosInstance.get(`/shift-templates/${id}`)
        return res.data
    },

    // Tạo mẫu ca mới
    create: async (req: CreateShiftTemplateRequest): Promise<void> => {
        await axiosInstance.post("/shift-templates", req)
    },

    // Cập nhật mẫu ca
    update: async ({ id, req }: { id: string; req: UpdateShiftTemplateRequest }): Promise<void> => {
        await axiosInstance.patch(`/shift-templates/${id}`, req)
    },

    // Xóa mẫu ca
    delete: async ({ id }: { id: string }): Promise<void> => {
        await axiosInstance.delete(`/shift-templates/${id}`)
    }
}
