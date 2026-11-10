import axiosInstance from "@/utils/axios"
import { UserUpdateReq } from "../models/request"
import { User } from "@/models/user"
import { LoyaltyAccountResponse, MembershipTier } from "@/models/loyalty"

/**
 * PROFILE API
 * Lưu ý: Logic tự động Logout khi 404 hoặc Token không hợp lệ
 * nên được xử lý tập trung ở Interceptor của axiosInstance để code ở đây luôn sạch.
 */
export const profileApi = {
    // Lấy thông tin cá nhân của người dùng hiện tại
    getMe: async (): Promise<User> => {
        const res = await axiosInstance.get("/users/me")
        return res.data
    },

    // Cập nhật thông tin cá nhân (Họ tên, số điện thoại, avatar...)
    updateMe: async (req: UserUpdateReq): Promise<void> => {
        await axiosInstance.patch("/users/me", req)
    },

    uploadAvatar: async (formData: FormData): Promise<string> => {
        const res = await axiosInstance.put("/users/me/avatar", formData, {
            headers: { "Content-Type": "multipart/form-data" }
        })
        return res.data
    },

    deleteAvatar: async (): Promise<void> => {
        await axiosInstance.delete("/users/me/avatar")
    },

    // Lấy loyalty tier của user hiện tại theo email
    getMyLoyalty: async (email: string): Promise<LoyaltyAccountResponse[]> => {
        const res = await axiosInstance.get("/loyalty/accounts", {
            params: { email }
        })
        return res.data
    },

    // Lấy danh sách tất cả các hạng thành viên
    getMembershipTiers: async (): Promise<MembershipTier[]> => {
        const res = await axiosInstance.get("/membership-tiers")
        return res.data
    },

    // Lấy lịch sử điểm của user hiện tại
    getMyPointLogs: async (params?: any): Promise<any> => {
        // Dùng PaginationResult theo chuẩn chung của BE (data.items)
        const res = await axiosInstance.get("/me/points/logs", { params })
        return res.data
    }
}

