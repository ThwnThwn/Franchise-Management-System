import { BACKEND_API_URL } from "@/constants/env"
import { useTokenStore } from "@/hooks/singleton/store"
import { BackendError } from "@/models/common/respone"
import axios from "axios"
import qs from "qs"

const axiosInstance = axios.create({
    baseURL: BACKEND_API_URL,
    withCredentials: true,
    headers: { "Content-Type": "application/json" },
    paramsSerializer: (params) => {
        // arrayFormat: 'repeat' sẽ tạo ra: ?ids=1&ids=2 (Chuẩn cho Gin/Golang)
        // arrayFormat: 'brackets' sẽ tạo ra: ?ids[]=1&ids[]=2 (Chuẩn mặc định Axios)
        return qs.stringify(params, { arrayFormat: "repeat" })
    }
})

const refreshInstance = axios.create({
    baseURL: BACKEND_API_URL,
    withCredentials: true
})

// request interceptors
axiosInstance.interceptors.request.use(
    (config) => {
        const token = useTokenStore.getState().accessToken
        if (token) {
            config.headers = config.headers || {}
            config.headers["Authorization"] = `Bearer ${token}`
        }
        return config
    },
    (error) => Promise.reject(error)
)

// response interceptors
axiosInstance.interceptors.response.use(
    (res) => res,
    async (error) => {
        const originalRequest = error.config

        // --- 1. Logic Refresh Token ---
        if (
            error.response?.status === 401 &&
            error.response?.data.detail === "4011" &&
            !originalRequest.sent
        ) {
            originalRequest.sent = true
            try {
                const res = await refreshInstance.post("/auth/refresh-token")
                const newToken = res.data.accessToken

                useTokenStore.getState().setAccessToken(newToken)

                // Gán lại token mới cho request bị lỗi trước đó
                originalRequest.headers["Authorization"] = `Bearer ${newToken}`
                return axiosInstance(originalRequest)
            } catch (refreshError) {
                // Nếu refresh cũng lỗi (hết hạn hoàn toàn) thì tiễn khách
                useTokenStore.getState().removeAccessToken()
                return Promise.reject(refreshError)
            }
        }

        // --- 2. Định dạng BackendError ---
        let backendError: BackendError = {
            title: "Lỗi hệ thống",
            status: 500,
            detail: "Xảy ra lỗi bất thường"
        }

        if (axios.isAxiosError(error)) {
            const data = error.response?.data
            backendError = {
                title: data?.title ?? "Lỗi hệ thống",
                status: data?.status ?? error.response?.status ?? 500,
                detail: data?.detail ?? error.message
            }

            // --- 3. Logic Logout đặc thù ---
            // Nếu get profile mà 404 (user bị xóa) hoặc 401 không thể refresh
            const isMeApi = originalRequest.url?.endsWith("/users/me")
            if (backendError.status === 404 && isMeApi) {
                useTokenStore.getState().removeAccessToken()
            }
        }

        return Promise.reject(backendError)
    }
)

export default axiosInstance
