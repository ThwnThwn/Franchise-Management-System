import axiosInstance from "@/utils/axios"
import { UserCurrentShift } from "@/features/shifts/models/response"
import { AttendancePayload } from "@/features/shifts/models/request"

/**
 * MY SHIFT API
 * @title Chấm công & Lịch cá nhân
 * @description Resource-based: /assignments/{id}/check-in và /logs/{id}/check-out
 */
export const myShiftApi = {
    /**
     * Lấy thông tin ca làm việc hiện tại của "Tôi"
     */
    getCurrent: async (): Promise<UserCurrentShift> => {
        const res = await axiosInstance.get("/me/shifts/current")
        return res.data
    },

    /**
     * CHECK-IN (Vào ca)
     * Thao tác trên thực thể: Assignment (Việc phân công)
     */
    checkIn: async (assignmentId: string, payload: AttendancePayload): Promise<void> => {
        await axiosInstance.put(`/me/shifts/assignments/${assignmentId}/check-in`, payload)
    },

    /**
     * CHECK-OUT (Tan ca)
     * Thao tác trên thực thể: Log (Bản ghi chấm công)
     */
    checkOut: async (logId: string, payload: AttendancePayload): Promise<void> => {
        await axiosInstance.put(`/me/shifts/logs/${logId}/check-out`, payload)
    }
}
