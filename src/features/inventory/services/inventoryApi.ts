import { buildQueryParams, requestWrapper } from "@/utils/helpers/axiosHelper"
import axiosInstance from "@/utils/axios"
import { GetAllStockParams, AdjustStockRequest } from "../models/request"
import { GetAllStockResult } from "../models/response"

export const inventoryApi = {
    getAllStocks: (params?: GetAllStockParams) =>
        requestWrapper<GetAllStockResult>(async () => {
            const queryParams = buildQueryParams(params ?? {})
            const res = await axiosInstance.get("/inventory/stocks", { params: queryParams })
            return res.data
        }),

    adjustStock: (req: AdjustStockRequest) =>
        requestWrapper<void>(async () => {
            await axiosInstance.post("/inventory/adjustments", req)
        })
}
