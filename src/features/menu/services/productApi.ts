import { buildQueryParams, requestWrapper } from "@/utils/helpers/axiosHelper"
import {
    CreateProductRequest,
    GetAllProductParams,
    UpdateProductRequest
} from "@/features/menu/models/products/request"
import axiosInstance from "@/utils/axios"
import { Product } from "../models/products/respone"
import { PaginationParams } from "@/models/common/request"
import { PageResult } from "@/models/common/respone"

export const productApi = {
    getAll: ({ query, pagination }: { query: GetAllProductParams; pagination: PaginationParams }) =>
        requestWrapper<PageResult<Product>>(async () => {
            const params = {
                ...buildQueryParams(query),
                ...buildQueryParams(pagination)
            }
            const res = await axiosInstance.get("/products", { params })
            return res.data
        }),

    getById: ({ id }: { id: string }) =>
        requestWrapper<Product>(async () => {
            const res = await axiosInstance.get(`/products/${id}`)
            return res.data
        }),

    create: (req: CreateProductRequest) =>
        requestWrapper<void>(async () => {
            // const res = await axiosInstance.post("/products", req)
            // return res.data
            await axiosInstance.post("/products", req)
        }),

    update: ({ id, req }: { id: string; req: UpdateProductRequest }) =>
        requestWrapper<void>(async () => {
            await axiosInstance.patch(`/products/${id}`, req)
        }),

    delete: ({ id }: { id: string }) =>
        requestWrapper<void>(async () => {
            await axiosInstance.delete(`/products/${id}`)
        }),

    uploadImage: ({ id, formData }: { id: string; formData: FormData }) =>
        requestWrapper<string>(async () => {
            const res = await axiosInstance.post(`/products/${id}/images`, formData, {
                headers: { "Content-Type": "multipart/form-data" }
            })
            return res.data
        })
}
