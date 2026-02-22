import { useQuery } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { categoryApi } from "../services"

export const useGetAllCategories = () => {
    return useQuery({
        queryKey: [QUERY_KEYS.CATEGORIES],
        queryFn: async () => {
            const data = await categoryApi.getAll()
            return data
        }
    })
}
