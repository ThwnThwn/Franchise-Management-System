import axiosInstance from "@/utils/axios"
import { StaffShiftAgenda, UserCurrentShift } from "@/features/shifts/models/response"
import { AttendancePayload, GetAgendaParams } from "@/features/shifts/models/request"

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
    },

    /**
     * Lấy danh sách lịch làm việc (Agenda) trong một khoảng thời gian nhất định.
     * Thường dùng để hiển thị Timeline tuần hoặc Lịch tháng cho nhân viên.
     * @param params - { startDate: string, endDate: string } Định dạng YYYY-MM-DD
     * @returns StaffShiftAgenda[] - Danh sách các ca làm, bao gồm thông tin Store và đồng nghiệp làm cùng.
     */
    getAgenda: async (params: GetAgendaParams): Promise<StaffShiftAgenda[]> => {
        const res = await axiosInstance.get("/me/shifts", { params })
        return res.data
    }
}
