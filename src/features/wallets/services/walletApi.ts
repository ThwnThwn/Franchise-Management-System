import axiosInstance from "@/utils/axios"
import { TopUpWalletRequest, UpdateWalletStatusRequest } from "@/features/wallets/models/request"
import { WalletResponse } from "@/features/wallets/models/response"

/**
 * WALLET API
 * Quản lý ví cá nhân, nạp tiền và điều chỉnh trạng thái ví.
 */
export const walletApi = {
    // [END-USER] Lấy thông tin ví của chính mình
    getMyWallet: async (): Promise<WalletResponse> => {
        const res = await axiosInstance.get("/me/wallet")
        return res.data
    },

    // [END-USER] Nạp tiền - Trả về URL để redirect sang VNPay
    topUpMyWallet: async (req: TopUpWalletRequest): Promise<{ paymentUrl: string }> => {
        const res = await axiosInstance.post("/me/wallet/top-up", req)
        return res.data
    },

    // [ADMIN] Xem ví của một người dùng bất kỳ theo UserId
    getByUserId: async (userId: string): Promise<WalletResponse> => {
        const res = await axiosInstance.get(`/wallets/users/${userId}`)
        return res.data
    },

    // [ADMIN] Khóa/Mở khóa ví
    updateStatus: async ({
        id,
        req
    }: {
        id: string
        req: UpdateWalletStatusRequest
    }): Promise<{ message: string }> => {
        const res = await axiosInstance.patch(`/wallets/${id}/status`, req)
        return res.data
    }
}
