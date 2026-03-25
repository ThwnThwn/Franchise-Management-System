import axiosInstance from "@/utils/axios"
import { buildQueryParams } from "@/utils/helpers/axiosHelper"
import {
    GetAllApplicationsRequest,
    ReviewFranchiseApplicationRequest,
    GetAllContractsRequest,
    ConfirmDepositPaymentRequest,
    UpdateSetupStatusRequest,
    UpdateContractStatusRequest,
    RenewContractRequest
} from "@/features/franchise/models/request"
import {
    FranchiseApplicationResponse,
    FranchiseApplicationDetailResponse,
    FranchiseContractResponse,
    ReviewApplicationResponse
} from "@/features/franchise/models/response"
import { PageResult } from "@/models/common/respone"

export const franchiseApi = {
    // ── APPLICATIONS ─────────────────────────────────────────────────────────

    /** Lấy danh sách hồ sơ (Admin) */
    getAllApplications: async (
        query: GetAllApplicationsRequest
    ): Promise<PageResult<FranchiseApplicationResponse>> => {
        const params = buildQueryParams(query)
        const res = await axiosInstance.get("/franchise/applications", { params })
        return res.data
    },

    /** Xem chi tiết hồ sơ */
    getApplicationDetail: async (id: string): Promise<FranchiseApplicationDetailResponse> => {
        const res = await axiosInstance.get(`/franchise/applications/${id}`)
        return res.data
    },

    /**
     * Nộp hồ sơ đăng ký nhượng quyền (Multipart/Form-data)
     * Hàm này nhận trực tiếp FormData vì mình tự build FormData ở Component
     */
    submitApplication: async (formData: FormData): Promise<FranchiseApplicationResponse> => {
        const res = await axiosInstance.post("/franchise/applications", formData, {
            headers: { "Content-Type": "multipart/form-data" }
        })
        return res.data
    },

    /** Admin duyệt / từ chối hồ sơ */
    reviewApplication: async ({
        id,
        req
    }: {
        id: string
        req: ReviewFranchiseApplicationRequest
    }): Promise<ReviewApplicationResponse> => {
        const res = await axiosInstance.patch(`/franchise/applications/${id}/review`, req)
        return res.data
    },

    // ── CONTRACTS ────────────────────────────────────────────────────────────

    /** Lấy danh sách hợp đồng (Admin/Kế toán) */
    getAllContracts: async (
        query: GetAllContractsRequest
    ): Promise<PageResult<FranchiseContractResponse>> => {
        const params = buildQueryParams(query)
        const res = await axiosInstance.get("/franchise/contracts", { params })
        return res.data
    },

    /** Xem chi tiết hợp đồng */
    getContractDetail: async (id: string): Promise<FranchiseContractResponse> => {
        const res = await axiosInstance.get(`/franchise/contracts/${id}`)
        return res.data
    },

    /** Kế toán xác nhận đóng tiền cọc */
    confirmPayment: async ({
        id,
        req
    }: {
        id: string
        req: ConfirmDepositPaymentRequest
    }): Promise<{ message?: string }> => {
        const res = await axiosInstance.post(`/franchise/contracts/${id}/payment`, req)
        return res.data
    },

    /** Cập nhật tiến độ setup cửa hàng */
    updateSetupStatus: async ({
        id,
        req
    }: {
        id: string
        req: UpdateSetupStatusRequest
    }): Promise<FranchiseContractResponse> => {
        const res = await axiosInstance.patch(`/franchise/contracts/${id}/setup`, req)
        return res.data
    },

    /** Tạm ngưng / Chấm dứt hợp đồng */
    updateContractStatus: async ({
        id,
        req
    }: {
        id: string
        req: UpdateContractStatusRequest
    }): Promise<FranchiseContractResponse> => {
        const res = await axiosInstance.patch(`/franchise/contracts/${id}/status`, req)
        return res.data
    },

    /** Gia hạn hợp đồng */
    renewContract: async ({
        id,
        req
    }: {
        id: string
        req: RenewContractRequest
    }): Promise<FranchiseContractResponse> => {
        const res = await axiosInstance.post(`/franchise/contracts/${id}/renew`, req)
        return res.data
    },

    // ME
    getMyApplications: async (): Promise<FranchiseApplicationResponse[]> => {
        const res = await axiosInstance.get("/franchise/me/applications")
        return res.data
    },

    getMyContracts: async (): Promise<FranchiseContractResponse[]> => {
        const res = await axiosInstance.get("/franchise/me/contracts")
        return res.data
    }
}
