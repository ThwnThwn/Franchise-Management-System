import { dehydrate, HydrationBoundary } from "@tanstack/react-query"
import MenuPage from "@/features/products/pages/MenuPage"
import { categoryApi, productApi } from "@/features/products/services"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { PageResult } from "@/models/common/respone"
import { Product } from "@/features/products/models/products/respone"
import { getQueryClient } from "@/utils/helpers/getQueryClient"

export const revalidate = 60

export default async function Menu() {
    const queryClient = getQueryClient()

    // 1. PREFETCH CATEGORIES (Danh mục)
    // Dùng prefetchQuery, nếu lỗi nó sẽ im lặng bỏ qua, không làm sập build
    await queryClient.prefetchQuery({
        queryKey: [QUERY_KEYS.CATEGORIES],
        queryFn: async () => {
            try {
                return await categoryApi.getAll()
            } catch {
                return []
            }
        }
    })

    // 2. PREFETCH SẢN PHẨM TRANG 1 (Infinite Query)
    // params và pageSize ở đây PHẢI GIỐNG Y HỆT state mặc định ở Client
    const initialParams = {}
    const initialPageSize = 4

    await queryClient.prefetchInfiniteQuery({
        queryKey: [QUERY_KEYS.PRODUCTS, initialParams, "infinite"],
        queryFn: async ({ pageParam = 1 }): Promise<PageResult<Product>> => {
            try {
                return await productApi.getAll({
                    pagination: { page: pageParam, pageSize: initialPageSize },
                    query: initialParams
                })
            } catch {
                return {
                    items: [],
                    totalCount: 0,
                    totalPages: 0
                }
            }
        },
        initialPageParam: 1
    })

    return (
        <HydrationBoundary state={dehydrate(queryClient)}>
            <MenuPage />
        </HydrationBoundary>
    )
}
