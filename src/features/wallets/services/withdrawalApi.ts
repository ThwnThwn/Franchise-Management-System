import { buildQueryParams } from "@/utils/helpers/axiosHelper"
import axiosInstance from "@/utils/axios"
import {
    CreateWithdrawalReq,
    GetMyWithdrawalsReq,
    CancelMyWithdrawalReq,
    GetAllWithdrawalsReq,
    ApproveWithdrawalReq,
    RejectWithdrawalReq,
    CancelWithdrawalReq,
    CompleteWithdrawalReq
} from "@/features/wallets/models/request"
import {
    WithdrawalBaseRes,
    WithdrawalSummary,
    WithdrawalDetailRes
} from "@/features/wallets/models/response"
import { PageResult } from "@/models/common/respone"

/**
 * WITHDRAWAL API
 * Quản lý yêu cầu rút tiền cho cả người dùng và quản trị viên.
 */
export const withdrawalApi = {
    // ----------------------------------------------------------------------
    // NHÓM END-USER (KHÁCH HÀNG)
    // ----------------------------------------------------------------------

    // Tạo yêu cầu rút tiền mới
    create: async (req: CreateWithdrawalReq): Promise<WithdrawalBaseRes> => {
        const res = await axiosInstance.post("/me/withdrawals", req)
        return res.data
    },

    // Xem lịch sử rút tiền cá nhân
    getMyWithdrawals: async (
        query: GetMyWithdrawalsReq
    ): Promise<PageResult<WithdrawalSummary>> => {
        const params = buildQueryParams(query)
        const res = await axiosInstance.get("/me/withdrawals", { params })
        return res.data
    },

    // Khách hàng tự hủy lệnh (Chỉ khi trạng thái là PENDING)
    cancelMyWithdrawal: async ({
        id,
        req
    }: {
        id: string
        req: CancelMyWithdrawalReq
    }): Promise<{ message: string }> => {
        const res = await axiosInstance.patch(`/me/withdrawals/${id}/cancel`, req)
        return res.data
    },

    // ----------------------------------------------------------------------
    // NHÓM ADMIN (KẾ TOÁN / QUẢN TRỊ VIÊN)
    // ----------------------------------------------------------------------

    // Lấy toàn bộ danh sách rút tiền hệ thống
    getAll: async (query: GetAllWithdrawalsReq): Promise<PageResult<WithdrawalSummary>> => {
        const params = buildQueryParams(query)
        const res = await axiosInstance.get("/withdrawals", { params })
        return res.data
    },

    // Xem chi tiết yêu cầu rút tiền (bao gồm Raw Account Number để chuyển khoản)
    getById: async (id: string): Promise<WithdrawalDetailRes> => {
        const res = await axiosInstance.get(`/withdrawals/${id}`)
        return res.data
    },

    // Duyệt yêu cầu rút tiền (Chuyển sang APPROVED)
    approve: async ({
        id,
        req
    }: {
        id: string
        req: ApproveWithdrawalReq
    }): Promise<{ message: string }> => {
        const res = await axiosInstance.patch(`/withdrawals/${id}/approve`, req)
        return res.data
    },

    // Từ chối yêu cầu (Hoàn tiền vào ví)
    reject: async ({
        id,
        req
    }: {
        id: string
        req: RejectWithdrawalReq
    }): Promise<{ message: string }> => {
        const res = await axiosInstance.patch(`/withdrawals/${id}/reject`, req)
        return res.data
    },

    // Admin hủy yêu cầu (Hoàn tiền vào ví)
    cancel: async ({
        id,
        req
    }: {
        id: string
        req: CancelWithdrawalReq
    }): Promise<{ message: string }> => {
        const res = await axiosInstance.patch(`/withdrawals/${id}/cancel`, req)
        return res.data
    },

    /**
     * Hoàn tất yêu cầu rút tiền (Xác nhận đã chuyển khoản thành công)
     * Sử dụng FormData để gửi mã giao dịch và file ảnh biên lai.
     */
    complete: async ({
        id,
        req
    }: {
        id: string
        req: CompleteWithdrawalReq
    }): Promise<{ message: string }> => {
        const formData = new FormData()
        formData.append("bankTransactionId", req.bankTransactionId)

        if (req.receiptFile) {
            formData.append("receiptFile", req.receiptFile)
        }

        const res = await axiosInstance.patch(`/withdrawals/${id}/complete`, formData, {
            headers: {
                "Content-Type": "multipart/form-data"
            }
        })
        return res.data
    }
}
