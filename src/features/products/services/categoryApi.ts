import axiosInstance from "@/utils/axios"
import { Category } from "../models/categories/response"
import { CreateCategoryRequest, UpdateCategoryRequest } from "../models/categories/request"

/**
 * CATEGORY API
 * Danh mục sản phẩm/dịch vụ
 * Endpoints: GET/POST /categories, PATCH/DELETE /categories/:id
 */
export const categoryApi = {
    // Lấy toàn bộ danh sách danh mục
    getAll: async (): Promise<Category[]> => {
        const res = await axiosInstance.get("/categories")
        return res.data
    },

    // Lấy chi tiết danh mục theo ID
    getById: async (id: string): Promise<Category> => {
        const res = await axiosInstance.get(`/categories/${id}`)
        return res.data
    },

    // Tạo danh mục mới
    create: async (req: CreateCategoryRequest): Promise<void> => {
        await axiosInstance.post("/categories", req)
    },

    // Cập nhật danh mục
    update: async (id: string, req: UpdateCategoryRequest): Promise<void> => {
        await axiosInstance.patch(`/categories/${id}`, req)
    },

    // Xóa danh mục
    delete: async (id: string): Promise<void> => {
        await axiosInstance.delete(`/categories/${id}`)
    }
}
