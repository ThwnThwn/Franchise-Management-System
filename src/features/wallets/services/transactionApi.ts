import { buildQueryParams } from "@/utils/helpers/axiosHelper"
import axiosInstance from "@/utils/axios"
import { GetAllWalletTransactionsRequest } from "@/features/wallets/models/request"
import { WalletTransactionResponse } from "@/features/wallets/models/response"
import { PageResult } from "@/models/common/respone"

export const transactionApi = {
    // [END-USER] Xem lịch sử của chính mình
    getMyTransactions: async (
        query: GetAllWalletTransactionsRequest
    ): Promise<PageResult<WalletTransactionResponse>> => {
        const params = buildQueryParams(query)
        const res = await axiosInstance.get("/me/wallet/transactions", { params })
        return res.data
    },

    // [ADMIN] Lấy toàn bộ giao dịch (có thể lọc theo userId qua query params)
    getAll: async (
        query: GetAllWalletTransactionsRequest & { userId?: string }
    ): Promise<PageResult<WalletTransactionResponse>> => {
        const params = buildQueryParams(query)
        const res = await axiosInstance.get("/wallets/transactions", { params })
        return res.data
    },

    // Lấy chi tiết bằng ID
    getById: async (id: string): Promise<WalletTransactionResponse> => {
        const res = await axiosInstance.get(`/wallets/transactions/${id}`)
        return res.data
    },

    // Lấy chi tiết bằng Mã (Code)
    getByCode: async (code: string): Promise<WalletTransactionResponse> => {
        const res = await axiosInstance.get(`/wallets/transactions/code/${code}`)
        return res.data
    }
}
