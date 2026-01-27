import { buildQueryParams, requestWrapper } from "@/utils/helpers/axiosHelper"
import { GetAllProductParams } from "@/features/menu/models/schema/request"
import { getProductsResultDto } from "@/features/menu/models/schema/respone"
import axiosInstance from "@/utils/axios"

export const productApi = {
    getAll: (query?: GetAllProductParams) =>
        requestWrapper<getProductsResultDto>(async () => {
            const params = buildQueryParams(query ?? {})
            const res = await axiosInstance.get("/products", { params })
            return res.data
        })
}
