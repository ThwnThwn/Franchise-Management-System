import { QUERY_KEYS } from "@/constants/queryKeys"
import { useQuery } from "@tanstack/react-query"
import { recipeApi } from "@/features/products/services"
import { GetRecipesReq } from "@/features/products/models/recipes/request"

export const useGetRecipes = ({
    req,
    enabled = true
}: {
    req: GetRecipesReq
    enabled?: boolean
}) => {
    return useQuery({
        queryKey: [QUERY_KEYS.RECIPES, req],
        queryFn: async () => {
            const data = await recipeApi.getAll(req)
            return data
        },
        enabled: enabled
    })
}
