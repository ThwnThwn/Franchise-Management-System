import { GetAllProductParams } from "../models/products/request"
import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { productApi } from "../services/productApi"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"

export const useGetAllProducts = ({
    query
}: // enabled = true
    {
        query?: GetAllProductParams
        // enabled?: boolean
    } = {}) => {

    // ...

    return useQuery({
        queryKey: [...QUERY_KEYS.PRODUCTS, query],
        queryFn: async () => {
            const data = await productApi.getAll(query)
            return data
        },
        placeholderData: keepPreviousData
    })
}

export const useGetProductById = ({ id }: { id: string }) => {
    return useQuery({
        queryKey: [...QUERY_KEYS.PRODUCTS, id],
        queryFn: async () => {
            return await productApi.getById({ id })
        }
    })
}

export const useCreateProduct = () => {
    const queryClient = useQueryClient()
    const key = QUERY_KEYS.PRODUCTS

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
    const key = QUERY_KEYS.PRODUCTS

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
    const key = QUERY_KEYS.PRODUCTS

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
