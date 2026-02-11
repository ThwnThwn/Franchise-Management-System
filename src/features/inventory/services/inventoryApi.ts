import { buildQueryParams } from "@/utils/helpers/axiosHelper"
import axiosInstance from "@/utils/axios"
import { GetAllStockParams, AdjustStockRequest } from "../models/request"
import { GetAllStockResult } from "../models/response"

/**
 * INVENTORY API
 * Xử lý các nghiệp vụ liên quan đến kho và tồn kho.
 */
export const inventoryApi = {
    /**
     * Lấy danh sách tồn kho của các cửa hàng
     */
    getAllStocks: async (params?: GetAllStockParams): Promise<GetAllStockResult> => {
        // Xử lý query params an toàn, fallback về object rỗng nếu không có params
        const queryParams = buildQueryParams(params ?? {})
        const res = await axiosInstance.get("/stores/stock", { params: queryParams })
        return res.data
    },

    /**
     * Điều chỉnh kho (Nhập/Xuất/Kiểm kê)
     */
    adjustStock: async (req: AdjustStockRequest): Promise<void> => {
        await axiosInstance.post("/inventory/adjustments", req)
    }
}
