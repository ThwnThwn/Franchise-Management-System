import { requestWrapper } from "@/utils/helpers/axiosHelper"
import { UserProfileViewRes } from "../models/respone"
import axiosInstance from "@/utils/axios"
import axios from "axios"
import { BackendError } from "@/models/common/respone"
import { useTokenStore } from "@/hooks"
import { UserUpdateReq } from "../models/request"

export const profileApi = {
    // me
    getMe: () =>
        requestWrapper<UserProfileViewRes>(async () => {
            try {
                const res = await axiosInstance.get("/me")
                return res.data
            } catch (error: unknown) {
                if (axios.isAxiosError(error)) {
                    const data = error.response?.data
                    const backendError: BackendError = {
                        title: data?.title ?? "Internal Server Error",
                        status: data?.status ?? error.response?.status,
                        detail: data?.detail ?? error.message
                    }

                    if (backendError.status == 404) {
                        useTokenStore.getState().removeAccessToken()
                    }
                    throw backendError
                }
                throw error
            }
        }),

    updateMe: (req: UserUpdateReq) =>
        requestWrapper<void>(async () => {
            await axiosInstance.patch("/me", req)
        })
}
