import { requestWrapper } from "@/utils/helpers/axiosHelper"
import axiosInstance from "@/utils/axios"
import { TokenRes } from "@/features/auth/models/TokenRes"

export const authApi = {
    login: ({ emailOrUsername, password }: { emailOrUsername: string; password: string }) =>
        requestWrapper<TokenRes>(async () => {
            const res = await axiosInstance.post("/auth/login", { emailOrUsername, password })
            return res.data
        })
}
