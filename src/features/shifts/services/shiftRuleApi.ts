import axiosInstance from "@/utils/axios"
import { ShiftRuleSettingsResponse } from "../models/response"
import { SaveShiftRuleRequest } from "../models/request"

/**
 * SHIFT RULE API
 * Quản lý luật lệ ca làm (Global & Store Specific)
 */
export const shiftRuleApi = {
    // Lấy cấu hình cho trang Settings (Manager/Admin)
    getSettings: async ({ storeId }: { storeId?: string }): Promise<ShiftRuleSettingsResponse> => {
        const res = await axiosInstance.get("/shift-rules", {
            params: { storeId }
        })
        return res.data
    },

    save: async (req: SaveShiftRuleRequest): Promise<void> => {
        await axiosInstance.put("/shift-rules", req)
    }
}
