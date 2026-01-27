import { GetAllProductParams } from "../models/schema/request"
import { useQuery } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { productApi } from "../services/productApi"

export const useGetAllProducts = ({
    query
}: // enabled = true
{
    query?: GetAllProductParams
    // enabled?: boolean
} = {}) => {
    return useQuery({
        queryKey: QUERY_KEYS.PRODUCTS,
        queryFn: async () => {
            const data = await productApi.getAll(query)
            return data
        }
        // enabled
    })
}
