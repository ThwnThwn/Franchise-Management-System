import { buildQueryParams } from "@/utils/helpers/axiosHelper"
import axiosInstance from "@/utils/axios"
import { PageResult } from "@/models/common/respone"
import {
    CreateShiftAvailabilityRequest,
    UpdateShiftAvailabilityRequest,
    GetAvailabilityParams
} from "../models/request"
import { ShiftAvailabilityResponse } from "../models/response"
import { PaginationParams } from "@/models/common/request"

/**
 * SHIFT AVAILABILITY API
 * Phục vụ trang quản lý lịch sử (Table) và đăng ký lịch rảnh/bận.
 */
export const shiftAvailabilityApi = {
    // --- STAFF SIDE (Dùng cho trang Table/History của nhân viên) ---

    // Lấy danh sách lịch bận/rảnh của chính tôi để hiện lên Table
    // grpc chỉ cần viết 1 hàm getAll là được, bỏ qua getMyHistory
    getMyHistory: async ({
        query,
        pagination
    }: {
        query: GetAvailabilityParams
        pagination: PaginationParams
    }): Promise<PageResult<ShiftAvailabilityResponse>> => {
        const params = {
            ...buildQueryParams(query),
            ...buildQueryParams(pagination)
        }
        const res = await axiosInstance.get("/me/shift-availabilities", { params })
        return res.data
    },

    // Tạo mới (Gửi đơn báo bận/rảnh)
    create: async (req: CreateShiftAvailabilityRequest): Promise<void> => {
        await axiosInstance.post("/shift-availabilities", req)
    },

    // Cập nhật (Sửa đơn đã gửi dựa trên ID)
    update: async ({
        id,
        req
    }: {
        id: string
        req: UpdateShiftAvailabilityRequest
    }): Promise<void> => {
        await axiosInstance.patch(`/shift-availabilities/${id}`, req)
    },

    // Xóa (Hủy đơn)
    delete: async (id: string): Promise<void> => {
        await axiosInstance.delete(`/shift-availabilities/${id}`)
    },

    // --- MANAGER SIDE ---

    // Lấy toàn bộ danh sách để Manager kiểm soát/phê duyệt
    getAll: async ({
        query,
        pagination
    }: {
        query: GetAvailabilityParams
        pagination: PaginationParams
    }): Promise<PageResult<ShiftAvailabilityResponse>> => {
        const params = {
            ...buildQueryParams(query),
            ...buildQueryParams(pagination)
        }
        const res = await axiosInstance.get("/shift-availabilities", { params })
        return res.data
    }
}
