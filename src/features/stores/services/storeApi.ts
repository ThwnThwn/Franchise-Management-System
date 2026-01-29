import { buildQueryParams, requestWrapper } from "@/utils/helpers/axiosHelper"
import axiosInstance from "@/utils/axios"
import { FindNearbyStoresParams, GetStoresParams } from "../models/schema/request"
import { GetStoresResponseDto, StoreDetailDto } from "../models/schema/response"

export const storeApi = {
    getAll: (query?: GetStoresParams) =>
        requestWrapper<GetStoresResponseDto>(async () => {
            const params = buildQueryParams(query ?? {})
            const res = await axiosInstance.get("/stores", { params })
            return res.data
        }),

    getDetail: (id: string) =>
        requestWrapper<StoreDetailDto>(async () => {
            const res = await axiosInstance.get(`/stores/${id}`)
            return res.data
        }),

    getNearby: (query: FindNearbyStoresParams) =>
        requestWrapper<GetStoresResponseDto>(async () => {
            const params = buildQueryParams(query)
            const res = await axiosInstance.get("/stores/nearby", { params })
            return res.data
        })
}
