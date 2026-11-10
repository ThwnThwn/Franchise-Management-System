import axiosInstance from "@/utils/axios"
import {
    BulkCreateFromTemplateRequest,
    BulkCreateManualRequest
} from "@/features/shifts/models/request"

/**
 * SHIFT BULK API
 * @title Thao tác hàng loạt trên Ca làm việc
 * @description Giảm số lượng request từ FE, hỗ trợ quản lý quy trình (Planning -> Publishing)
 */
export const shiftBulkApi = {
    // 1. Tạo ca thủ công hàng loạt theo khung giờ và các ngày trong tuần
    createManual: async (req: BulkCreateManualRequest): Promise<void> => {
        await axiosInstance.post("/shifts/bulk-manual", req)
    },

    // 2. Tạo ca hàng loạt dựa trên Template có sẵn
    createFromTemplate: async (req: BulkCreateFromTemplateRequest): Promise<void> => {
        await axiosInstance.post("/shifts/bulk-template", req)
    },

    // 3. Công khai hàng loạt ca làm việc (Chuyển Draft -> Published)
    // Hệ thống sẽ gửi thông báo tổng hợp 1 lần cho nhân viên.
    publish: async (ids: string[]): Promise<void> => {
        await axiosInstance.put("/shifts/bulk-publish", ids)
    },

    // 4. Xóa hàng loạt ca làm việc (Chỉ cho phép xóa các ca đang ở trạng thái Draft)
    delete: async (ids: string[]): Promise<void> => {
        // Dùng data trong config của axios delete để gửi mảng IDs
        await axiosInstance.delete("/shifts/bulk-delete", { data: ids })
    },

    open: async (ids: string[]): Promise<void> => {
        await axiosInstance.put("/shifts/bulk-open", ids)
    }
}
