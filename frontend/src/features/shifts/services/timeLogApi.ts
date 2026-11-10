import { buildQueryParams } from "@/utils/helpers/axiosHelper"
import axiosInstance from "@/utils/axios"
import { GetTimeLogsParams, UpdateTimeLogRequest } from "../models/request"
import { TimeLogResponse } from "../models/response"
import { PaginationParams } from "@/models/common/request"
import { PageResult } from "@/models/common/respone"

/**
 * SHIFT API
 * Quản lý lịch làm việc và phân công nhân sự.
 */
export const timeLogApi = {
    /**
     * 1. Xem toàn bộ nhật ký chấm công trong Store
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
     * 2. Duyệt công (Phê duyệt hoặc Từ chối)
     * @endpoint PATCH /shifts/time-logs/:id
     * @description Chỉ cập nhật Status và Note, không ghi đè dữ liệu gốc của nhân viên.
     */
    updateTimeLog: async (id: string, req: UpdateTimeLogRequest): Promise<void> => {
        await axiosInstance.patch(`/shifts/time-logs/${id}`, req)
    }
}
