import axiosInstance from "@/utils/axios"
import { RecipeViewRes } from "@/features/products/models/recipes/response"
import { GetRecipesReq } from "@/features/products/models/recipes/request"

/**
 * RECIPE API
 * Công thức
 */
export const recipeApi = {
    // Lấy toàn bộ danh sách danh mục
    getAll: async (req: GetRecipesReq): Promise<RecipeViewRes[]> => {
        const res = await axiosInstance.get("/recipes", {
            params: req
        })
        return res.data
    }
}
