import { requestWrapper } from "@/utils/helpers/axiosHelper"
import axiosInstance from "@/utils/axios"
import { TokenRes } from "../models/respone"
import { UserRegisterReq } from "../models/request"

export const authApi = {
    login: ({ email, password }: { email: string; password: string }) =>
        requestWrapper<TokenRes>(async () => {
            const res = await axiosInstance.post("/auth/login", { email, password })
            return res.data
        }),
    logout: () =>
        requestWrapper<void>(async () => {
            await axiosInstance.post("/auth/logout")
        }),
    // =======================
    // Register
    // =======================
    regsiter: (req: UserRegisterReq) =>
        requestWrapper<void>(async () => {
            const res = await axiosInstance.post("/auth/register", req)
            return res.data
        }),
    registerVerify: ({ otp, email }: { otp: string; email: string }) =>
        requestWrapper<void>(async () => {
            await axiosInstance.post("/auth/register/confirm-otp", { otp, email })
        })
}
