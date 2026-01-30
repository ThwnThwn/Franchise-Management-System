import { requestWrapper } from "@/utils/helpers/axiosHelper"
import axiosInstance from "@/utils/axios"
import { Category } from "../models/categories/response"

export const categoryApi = {
    getAll: () =>
        requestWrapper<Category[]>(async () => {
            const res = await axiosInstance.get("/categories")
            return res.data
        })
}
