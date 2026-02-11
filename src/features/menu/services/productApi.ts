import { buildQueryParams } from "@/utils/helpers/axiosHelper"
import {
    CreateProductRequest,
    GetAllProductParams,
    UpdateProductRequest
} from "@/features/menu/models/products/request"
import axiosInstance from "@/utils/axios"
import { Product } from "../models/products/respone"
import { PaginationParams } from "@/models/common/request"
import { PageResult } from "@/models/common/respone"

/**
 * PRODUCT API
 */
export const productApi = {
    // Lấy danh sách sản phẩm (Filter + Phân trang)
    getAll: async ({
        query,
        pagination
    }: {
        query: GetAllProductParams
        pagination: PaginationParams
    }): Promise<PageResult<Product>> => {
        const params = {
            ...buildQueryParams(query),
            ...buildQueryParams(pagination)
        }
        const res = await axiosInstance.get("/products", { params })
        return res.data
    },

    // Lấy chi tiết sản phẩm
    getById: async ({ id }: { id: string }): Promise<Product> => {
        const res = await axiosInstance.get(`/products/${id}`)
        return res.data
    },

    // Tạo sản phẩm mới
    create: async (req: CreateProductRequest): Promise<void> => {
        await axiosInstance.post("/products", req)
    },

    // Cập nhật thông tin sản phẩm
    update: async ({ id, req }: { id: string; req: UpdateProductRequest }): Promise<void> => {
        await axiosInstance.patch(`/products/${id}`, req)
    },

    // Xóa sản phẩm
    delete: async ({ id }: { id: string }): Promise<void> => {
        await axiosInstance.delete(`/products/${id}`)
    },

    // Upload hình ảnh sản phẩm (Xử lý FormData)
    uploadImage: async ({ id, formData }: { id: string; formData: FormData }): Promise<string> => {
        const res = await axiosInstance.post(`/products/${id}/images`, formData, {
            headers: { "Content-Type": "multipart/form-data" }
        })
        return res.data
    }
}
