import { useQuery } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { transactionApi } from "@/features/wallets/services"
import { GetAllWalletTransactionsRequest } from "@/features/wallets/models/request"
import { WalletTransactionResponse } from "@/features/wallets/models/response"
import { PageResult } from "@/models/common/respone"

/**
 * [Story: KHÁCH HÀNG xem lịch sử giao dịch của mình]
 * Endpoint: /me/wallet/transactions
 */
export const useGetMyTransactions = (query: GetAllWalletTransactionsRequest, enabled = true) => {
    return useQuery<PageResult<WalletTransactionResponse>>({
        queryKey: [QUERY_KEYS.WALLET_TRANSACTIONS, QUERY_KEYS.ME, query],
        queryFn: () => transactionApi.getMyTransactions(query),
        enabled
    })
}

/**
 * [Story: ADMIN xem danh sách giao dịch hệ thống]
 * Có thể truyền thêm userId vào query để lọc.
 * Endpoint: /wallets/transactions
 */
export const useGetAllTransactions = (
    query: GetAllWalletTransactionsRequest & { userId?: string },
    enabled = true
) => {
    return useQuery<PageResult<WalletTransactionResponse>>({
        queryKey: [QUERY_KEYS.WALLET_TRANSACTIONS, query],
        queryFn: () => transactionApi.getAll(query),
        enabled
    })
}

/**
 * [Story: Xem chi tiết giao dịch qua ID]
 */
export const useGetTransactionById = (id: string, enabled = true) => {
    return useQuery<WalletTransactionResponse>({
        queryKey: [QUERY_KEYS.WALLET_TRANSACTIONS, "detail", id],
        queryFn: () => transactionApi.getById(id),
        enabled: !!id && enabled
    })
}

/**
 * [Story: Xem chi tiết giao dịch qua Mã (Code)]
 */
export const useGetTransactionByCode = (code: string, enabled = true) => {
    return useQuery<WalletTransactionResponse>({
        queryKey: [QUERY_KEYS.WALLET_TRANSACTIONS, "code", code],
        queryFn: () => transactionApi.getByCode(code),
        enabled: !!code && enabled
    })
}
