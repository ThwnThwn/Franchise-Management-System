import { buildQueryParams } from "@/utils/helpers/axiosHelper"
import axiosInstance from "@/utils/axios"
import {
    GetAllStockParams,
    AdjustStockRequest,
    GetIngredientsParams,
    CreateIngredientRequest,
    UpdateIngredientRequest,
    AddIngredientStockItem,
    CreateIngredientCategoryRequest,
    UpdateIngredientCategoryRequest
} from "../models/request"
import { GetAllStockResult, GetIngredientsResult, IngredientCategory } from "../models/response"

/**
 * INVENTORY API
 * Xử lý các nghiệp vụ liên quan đến kho, tồn kho và nguyên liệu.
 */
export const inventoryApi = {
    // =============================================
    // STOCK
    // =============================================

    /** Lấy danh sách tồn kho tất cả cửa hàng */
    getAllStocks: async (params?: GetAllStockParams): Promise<GetAllStockResult> => {
        const queryParams = buildQueryParams(params ?? {})
        const res = await axiosInstance.get("/stores/stock", { params: queryParams })
        return res.data
    },

    /** Lấy tồn kho theo 1 cửa hàng cụ thể — GET /stores/:storeId/stock */
    getStockByStoreId: async (
        storeId: string,
        params?: Omit<GetAllStockParams, "storeIds">
    ): Promise<GetAllStockResult> => {
        const queryParams = buildQueryParams(params ?? {})
        const res = await axiosInstance.get(`/stores/${storeId}/stock`, { params: queryParams })
        return res.data
    },

    /** Điều chỉnh kho (legacy) */
    adjustStock: async (req: AdjustStockRequest): Promise<void> => {
        await axiosInstance.put(`/stores/${req.storeId}/stock`, req.items)
        // await axiosInstance.post("/inventory/adjustments", req)
    },

    /**
     * Nhập kho nguyên liệu cho một store cụ thể
     * PUT /stores/:storeId/stock — body: IngredientStockItem[]
     */
    increaseStoreStock: async (storeId: string, items: AddIngredientStockItem[]): Promise<void> => {
        await axiosInstance.put(`/stores/${storeId}/stock`, items)
    },

    // =============================================
    // INGREDIENTS — GET /ingredients, POST, PATCH/:id, DELETE/:id
    // =============================================

    /** GET /api/ingredients - Lấy danh sách nguyên liệu */
    getIngredients: async (params?: GetIngredientsParams): Promise<GetIngredientsResult> => {
        const queryParams = buildQueryParams(params ?? {})
        const res = await axiosInstance.get("/ingredients", { params: queryParams })
        return res.data
    },

    /** POST /api/ingredients - Tạo nguyên liệu mới */
    createIngredient: async (req: CreateIngredientRequest): Promise<void> => {
        await axiosInstance.post("/ingredients", req)
    },

    /** PATCH /api/ingredients/:id - Cập nhật nguyên liệu */
    updateIngredient: async (id: string, req: UpdateIngredientRequest): Promise<void> => {
        await axiosInstance.patch(`/ingredients/${id}`, req)
    },

    /** DELETE /api/ingredients/:id - Xóa nguyên liệu */
    deleteIngredient: async (id: string): Promise<void> => {
        await axiosInstance.delete(`/ingredients/${id}`)
    },

    // =============================================
    // INGREDIENT CATEGORIES — GET /ingredients/categories
    // =============================================

    /** GET /api/ingredients/categories - Lấy danh mục nguyên liệu */
    getIngredientCategories: async (): Promise<IngredientCategory[]> => {
        const res = await axiosInstance.get("/ingredients/categories")
        return res.data
    },

    /** POST /api/ingredients/categories - Tạo danh mục nguyên liệu */
    createIngredientCategory: async (req: CreateIngredientCategoryRequest): Promise<void> => {
        await axiosInstance.post("/ingredients/categories", req)
    },

    /** PATCH /api/ingredients/categories/:id - Cập nhật danh mục nguyên liệu */
    updateIngredientCategory: async (id: string, req: UpdateIngredientCategoryRequest): Promise<void> => {
        await axiosInstance.patch(`/ingredients/categories/${id}`, req)
    },

    /** DELETE /api/ingredients/categories/:id - Xóa danh mục nguyên liệu */
    deleteIngredientCategory: async (id: string): Promise<void> => {
        await axiosInstance.delete(`/ingredients/categories/${id}`)
    }
}
