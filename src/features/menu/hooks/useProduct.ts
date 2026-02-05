import { GetAllProductParams } from "../models/products/request"
import { useInfiniteQuery, useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { productApi } from "../services/productApi"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError, PageResult } from "@/models/common/respone"
import { PaginationParams } from "@/models/common/request"
import { Product } from "../models/products/respone"

export const useGetInfiniteProducts = ({
    params,
    pageSize = 4
}: {
    params: GetAllProductParams
    pageSize?: number
}) => {
    return useInfiniteQuery({
        queryKey: [QUERY_KEYS.PRODUCTS, params],
        queryFn: async ({ pageParam = 1 }) => {
            const response = await productApi.getAll({
                pagination: {
                    page: pageParam,
                    pageSize: pageSize
                },
                query: params
            })
            return response
        },
        initialPageParam: 1,
        getNextPageParam: (lastPage, allPages) => {
            // allPages.length chính là số trang hiện tại đã load thành công
            const currentPage = allPages.length

            // Nếu trang hiện tại vẫn nhỏ hơn tổng số trang backend báo về
            return currentPage < lastPage.totalPages ? currentPage + 1 : undefined
        }
    })
}

export const useGetAllProducts = ({
    params,
    pagination = {},
    enabled = true
}: {
    params: GetAllProductParams
    pagination: PaginationParams
    enabled?: boolean
}) => {
    const queryClient = useQueryClient()

    return useQuery({
        queryKey: [QUERY_KEYS.PRODUCTS, params],
        queryFn: () => productApi.getAll({ query: params, pagination }),
        initialData: () => {
            return queryClient.getQueryData<PageResult<Product>>([
                QUERY_KEYS.USERS,
                params,
                pagination
            ])
        },
        enabled
    })
}

export const useGetProductById = ({ id }: { id: string }) => {
    return useQuery({
        queryKey: [QUERY_KEYS.PRODUCTS, id],
        queryFn: async () => {
            return await productApi.getById({ id })
        }
    })
}

export const useCreateProduct = () => {
    const queryClient = useQueryClient()
    const key = [QUERY_KEYS.PRODUCTS]

    return useMutation({
        mutationFn: productApi.create,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: key })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Tạo sản phẩm thành công",
                color: "success"
            })
        },
        onError: (error: BackendError) => {
            addToast({
                title: error.title || STRINGS.COMMON.ERROR,
                description: error.detail,
                color: "danger"
            })
        }
    })
}

export const useUpdateProduct = () => {
    const queryClient = useQueryClient()
    const key = [QUERY_KEYS.PRODUCTS]

    return useMutation({
        mutationFn: productApi.update,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: key })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Cập nhật sản phẩm thành công",
                color: "success"
            })
        },
        onError: (error: BackendError) => {
            addToast({
                title: error.title || STRINGS.COMMON.ERROR,
                description: error.detail,
                color: "danger"
            })
        }
    })
}

export const useDeleteProduct = () => {
    const queryClient = useQueryClient()
    const key = [QUERY_KEYS.PRODUCTS]

    return useMutation({
        mutationFn: productApi.delete,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: key })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Xoá sản phẩm thành công",
                color: "success"
            })
        },
        onError: (error: BackendError) => {
            addToast({
                title: error.title || STRINGS.COMMON.ERROR,
                description: error.detail,
                color: "danger"
            })
        }
    })
}
