import { QUERY_KEYS } from "@/constants/queryKeys"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { variantApi } from "@/features/products/services"
import { UpdateRecipeItem } from "@/features/products/models/recipes/request"

/**
 * Mutation hook: Cập nhật công thức nguyên liệu cho một biến thể sản phẩm
 * Sau khi lưu, tự động invalidate cache của RECIPES để RecipeEditModal refetch dữ liệu mới
 */
export const useUpdateVariantRecipes = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: ({ variantId, items }: { variantId: string; items: UpdateRecipeItem[] }) =>
            variantApi.updateRecipes(variantId, items),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.RECIPES] })
        }
    })
}
