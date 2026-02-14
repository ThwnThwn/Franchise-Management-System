import { buildQueryParams } from "@/utils/helpers/axiosHelper"
import axiosInstance from "@/utils/axios"
import {
    CreateShiftRequest,
    GetShiftsParams,
    GetTimeLogsParams,
    UpdateShiftRequest,
    UpdateTimeLogRequest
} from "../models/request"
import { ShiftResponse, StaffAssignmentCandidate, TimeLogResponse } from "../models/response"
import { PaginationParams } from "@/models/common/request"
import { PageResult } from "@/models/common/respone"

/**
 * SHIFT API
 * Quản lý lịch làm việc và phân công nhân sự.
 */
export const shiftApi = {
    // Lấy danh sách ca làm việc (Calendar/List view)
    getAll: async ({ query }: { query: GetShiftsParams }): Promise<ShiftResponse[]> => {
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
        await axiosInstance.put(`/shifts/${id}/publish`)
    },

    /**
     * @param ids Mảng các shift id để publish đồng thời
     */
    // bulkPublish: async (ids: string[]): Promise<void> => {
    //     await axiosInstance.post("/shifts/bulk-publish", { ids })
    // },

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
        await axiosInstance.put(`/shifts/${shiftId}/assign`, userIds)
    },

    /**
     * 2. Xem toàn bộ nhật ký chấm công trong Store
     * @endpoint GET /shifts/time-logs
     * @access Manager
     */
    getAllTimeLogs: async ({
        query,
        pagination
    }: {
        query: GetTimeLogsParams
        pagination: PaginationParams
    }): Promise<PageResult<TimeLogResponse>> => {
        const params = {
            ...buildQueryParams(query),
            ...buildQueryParams(pagination)
        }
        const res = await axiosInstance.get("/shifts/time-logs", { params })
        return res.data
    },

    /**
     * 3. Duyệt công (Phê duyệt hoặc Từ chối)
     * @endpoint PATCH /shifts/time-logs/:id
     * @description Chỉ cập nhật Status và Note, không ghi đè dữ liệu gốc của nhân viên.
     */
    updateTimeLog: async (id: string, req: UpdateTimeLogRequest): Promise<void> => {
        await axiosInstance.patch(`/shifts/time-logs/${id}`, req)
    }
}
