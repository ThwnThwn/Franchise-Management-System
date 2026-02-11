import { buildQueryParams } from "@/utils/helpers/axiosHelper"
import axiosInstance from "@/utils/axios"
import { PageResult } from "@/models/common/respone"
import { CreateShiftRequest, GetShiftsParams, UpdateShiftRequest } from "../models/request"
import { ShiftResponse, StaffAssignmentCandidate } from "../models/response"

/**
 * SHIFT API
 * Quản lý lịch làm việc và phân công nhân sự.
 */
export const shiftApi = {
    // Lấy danh sách ca làm việc (Calendar/List view)
    getAll: async ({ query }: { query: GetShiftsParams }): Promise<PageResult<ShiftResponse>> => {
        const params = {
            ...buildQueryParams(query)
        }
        const res = await axiosInstance.get("/shifts", { params })
        return res.data
    },

    // Lấy chi tiết một ca làm (bao gồm cả danh sách nhân viên đã gán)
    getById: async ({ id }: { id: string }): Promise<ShiftResponse> => {
        const res = await axiosInstance.get(`/shifts/${id}`)
        return res.data
    },

    // Tạo ca làm mới
    create: async (req: CreateShiftRequest): Promise<void> => {
        await axiosInstance.post("/shifts", req)
    },

    // Cập nhật thông tin ca làm (Giờ giấc, số lượng nhân viên cần thiết)
    update: async ({ id, req }: { id: string; req: UpdateShiftRequest }): Promise<void> => {
        await axiosInstance.patch(`/shifts/${id}`, req)
    },

    // Xóa ca làm (Chỉ khi ở trạng thái Draft/Planning)
    delete: async ({ id }: { id: string }): Promise<void> => {
        await axiosInstance.delete(`/shifts/${id}`)
    },

    // Chuyển trạng thái sang Published để nhân viên thấy được lịch
    publish: async ({ id }: { id: string }): Promise<void> => {
        await axiosInstance.post(`/shifts/${id}/publish`)
    },

    /**
     * @param storeId - ID của cửa hàng để giới hạn danh sách nhân viên
     * @param shiftId - ID của ca làm để check conflict/availability/assigned
     */
    getAvailableStaffs: async ({
        storeId,
        shiftId
    }: {
        storeId: string
        shiftId: string
    }): Promise<StaffAssignmentCandidate[]> => {
        const res = await axiosInstance.get(`/stores/${storeId}/shifts/${shiftId}/candidates`)
        return res.data
    },

    /**
     * @param shiftId - ID của ca làm việc
     * @param userIds - Mảng các User ID cuối cùng sau khi Manager đã chỉnh sửa
     */
    updateAssignments: async ({
        shiftId,
        userIds
    }: {
        shiftId: string
        userIds: string[]
    }): Promise<void> => {
        // Backend sẽ so sánh mảng này với DB:
        // - User nào mới thì Insert, có mà đang isDeleted = true thì thành false
        // - User nào thiếu so với DB thì soft Delete
        await axiosInstance.put(`/shifts/${shiftId}/assign`, { userIds })
    }
}
