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
     * Lấy danh sách nhân viên tiềm năng để gán vào ca
     * Backend sẽ xử lý check Conflict và Availability dựa trên ShiftID
     */
    getAvailableStaffs: async (shiftId: string): Promise<StaffAssignmentCandidate[]> => {
        const res = await axiosInstance.get(`/shifts/${shiftId}/available-staffs`)
        return res.data
    },

    /**
     * Gán nhân viên vào ca
     */
    assignStaff: async ({ id, userIds }: { id: string; userIds: string[] }): Promise<void> => {
        await axiosInstance.post(`/shifts/${id}/assign`, { userIds })
    },

    /**
     * Gỡ nhân viên khỏi ca
     */
    removeStaff: async ({ id, userId }: { id: string; userId: string }): Promise<void> => {
        await axiosInstance.delete(`/shifts/${id}/assign/${userId}`)
    }
}
