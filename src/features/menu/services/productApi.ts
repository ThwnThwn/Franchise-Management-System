import { buildQueryParams, requestWrapper } from "@/utils/helpers/axiosHelper"
import { GetAllProductParams } from "@/features/menu/models/schema/request"
import axiosInstance from "@/utils/axios"
import { GetProductsResultDto } from "../models/schema/respone"

export const productApi = {
    getAll: (query?: GetAllProductParams) =>
        requestWrapper<GetProductsResultDto>(async () => {
            const params = buildQueryParams(query ?? {})
            const res = await axiosInstance.get("/products", { params })
            return res.data
        })
}
