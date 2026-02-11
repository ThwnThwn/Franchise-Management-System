import axiosInstance from "@/utils/axios"
import { UserProfileViewRes } from "../models/respone"
import { UserUpdateReq } from "../models/request"

/**
 * PROFILE API
 * Lưu ý: Logic tự động Logout khi 404 hoặc Token không hợp lệ
 * nên được xử lý tập trung ở Interceptor của axiosInstance để code ở đây luôn sạch.
 */
export const profileApi = {
    // Lấy thông tin cá nhân của người dùng hiện tại
    getMe: async (): Promise<UserProfileViewRes> => {
        const res = await axiosInstance.get("/users/me")
        return res.data
    },

    // Cập nhật thông tin cá nhân (Họ tên, số điện thoại, avatar...)
    updateMe: async (req: UserUpdateReq): Promise<void> => {
        await axiosInstance.patch("/users/me", req)
    }
}
