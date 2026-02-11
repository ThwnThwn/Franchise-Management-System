import axiosInstance from "@/utils/axios"
import { TokenRes } from "../models/respone"
import { UserRegisterReq } from "../models/request"

/**
 * AUTH API
 */
export const authApi = {
    // Login
    login: async ({
        emailOrUsername,
        password
    }: {
        emailOrUsername: string
        password: string
    }): Promise<TokenRes> => {
        const res = await axiosInstance.post("/auth/login", { emailOrUsername, password })
        return res.data
    },

    // Logout
    logout: async (): Promise<void> => {
        await axiosInstance.post("/auth/logout")
    },

    // =======================
    // Register
    // =======================
    register: async (req: UserRegisterReq): Promise<void> => {
        const res = await axiosInstance.post("/auth/register", req)
        return res.data
    },

    registerVerify: async ({ otp, email }: { otp: string; email: string }): Promise<void> => {
        await axiosInstance.post("/auth/register/confirm-otp", { otp, email })
    },

    // =======================
    // Forgot Password
    // =======================
    forgetPassword: async (email: string): Promise<void> => {
        await axiosInstance.post("/auth/forgot-password", { email })
    },

    forgotPasswordVerify: async ({ otp, email }: { otp: string; email: string }): Promise<void> => {
        await axiosInstance.post("/auth/forgot-password/confirm-otp", { otp, email })
    },

    resetPassword: async ({
        newPassword,
        confirmPassword
    }: {
        newPassword: string
        confirmPassword: string
    }): Promise<void> => {
        await axiosInstance.post("/auth/forgot-password/new-password", {
            newPassword,
            confirmPassword
        })
    },

    resendForgotPasswordOtp: async (email: string): Promise<void> => {
        await axiosInstance.post("/auth/forgot-password/resend-otp", { email })
    }
}
