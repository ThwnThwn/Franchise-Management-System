import { requestWrapper } from "@/utils/helpers/axiosHelper"
import { TokenRes } from "../models/tokenRes"
import axiosInstance from "@/utils/axios"

export const authApi = {
    login: ({ email, password }: { email: string; password: string }) =>
        requestWrapper<TokenRes>(async () => {
            const res = await axiosInstance.post("/auth/login", { email, password })
            return res.data
        })
}
