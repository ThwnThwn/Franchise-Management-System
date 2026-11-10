import { buildQueryParams } from "@/utils/helpers/axiosHelper"
import { FindNearbyStoresParams, GetStoresParams } from "../models/schema/request"
import axiosInstance from "@/utils/axios"
import { StoreDetailDto, ActiveStaffDto } from "../models/schema/response"
import { PaginationParams } from "@/models/common/request"
import { PageResult } from "@/models/common/respone"

/**
 * STORE API
 * Quản lý danh sách và chi tiết cửa hàng.
 */
export const storeApi = {
    // Lấy danh sách cửa hàng (Phân trang + Filter)
    getAll: async ({
        pagination,
        query
    }: {
        pagination: PaginationParams
        query: GetStoresParams
    }): Promise<PageResult<StoreDetailDto>> => {
        const params = {
            ...buildQueryParams(query),
            ...buildQueryParams(pagination)
        }
        const res = await axiosInstance.get("/stores", { params })
        return res.data
    },

    // Lấy chi tiết một cửa hàng
    getDetail: async (id: string): Promise<StoreDetailDto> => {
        const res = await axiosInstance.get(`/stores/${id}`)
        return res.data
    },

    // Tìm các cửa hàng gần đây (Dựa trên tọa độ GPS)
    getNearby: async (query: FindNearbyStoresParams): Promise<PageResult<StoreDetailDto>> => {
        const params = buildQueryParams(query)
        const res = await axiosInstance.get("/stores/nearby", { params })
        return res.data
    },

    getActiveStaffs: async (id: string): Promise<ActiveStaffDto[]> => {
        const res = await axiosInstance.get(`/stores/${id}/staffs/active`)
        return res.data
    }
}
