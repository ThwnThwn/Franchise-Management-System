import axiosInstance from "@/utils/axios"
import { buildQueryParams } from "@/utils/helpers/axiosHelper"
import { CreateSupplierRequest, GetSuppliersParams, UpdateSupplierRequest } from "../models/request"
import { GetSuppliersResult, SupplierDetail } from "../models/response"

export const supplierApi = {
    /** GET /suppliers - Danh sách nhà cung cấp */
    getSuppliers: async (params?: GetSuppliersParams): Promise<GetSuppliersResult> => {
        const queryParams = buildQueryParams(params ?? {})
        const res = await axiosInstance.get("/suppliers", { params: queryParams })
        return res.data
    },

    /** POST /suppliers - Tạo mới nhà cung cấp */
    createSupplier: async (req: CreateSupplierRequest): Promise<void> => {
        await axiosInstance.post("/suppliers", req)
    },

    /** GET /suppliers/:id - Chi tiết nhà cung cấp */
    getSupplierById: async (id: string): Promise<SupplierDetail> => {
        const res = await axiosInstance.get(`/suppliers/${id}`)
        return res.data
    },

    /** PATCH /suppliers/:id - Cập nhật nhà cung cấp */
    updateSupplier: async (id: string, req: Omit<UpdateSupplierRequest, "id">): Promise<void> => {
        await axiosInstance.patch(`/suppliers/${id}`, req)
    },

    /** DELETE /suppliers/:id - Xóa nhà cung cấp */
    deleteSupplier: async (id: string): Promise<void> => {
        await axiosInstance.delete(`/suppliers/${id}`)
    }
}
