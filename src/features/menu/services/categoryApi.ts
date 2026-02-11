import axiosInstance from "@/utils/axios"
import { Category } from "../models/categories/response"

/**
 * CATEGORY API
 * Danh mục sản phẩm/dịch vụ
 */
export const categoryApi = {
    // Lấy toàn bộ danh sách danh mục
    getAll: async (): Promise<Category[]> => {
        const res = await axiosInstance.get("/categories")
        return res.data
    }
}
