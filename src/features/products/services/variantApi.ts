import axiosInstance from "@/utils/axios"
import { RecipeViewRes } from "@/features/products/models/recipes/response"
import { UpdateRecipeItem } from "@/features/products/models/recipes/request"

/**
 * VARIANT API
 * Quản lý biến thể sản phẩm và công thức nguyên liệu của từng biến thể.
 */
export const variantApi = {
    /**
     * Lấy chi tiết một biến thể theo ID
     * GET /productVariants?id=:id
     */
    getById: async (id: string): Promise<RecipeViewRes[]> => {
        const res = await axiosInstance.get("/productVariants", { params: { id } })
        return res.data
    },

    /**
     * Cập nhật (ghi đè) công thức nguyên liệu cho một biến thể
     * PUT /variants/:id/recipes
     * BE sẽ so sánh và diff: insert mới, xóa cũ, giữ nguyên không thay đổi
     */
    updateRecipes: async (variantId: string, items: UpdateRecipeItem[]): Promise<void> => {
        await axiosInstance.put(`/variants/${variantId}/recipes`, items)
    }
}
