import { BACKEND_API_URL } from "@/constants/env"
import { useTokenStore } from "@/hooks"
import axios from "axios"

const axiosInstance = axios.create({
    baseURL: BACKEND_API_URL,
    withCredentials: true,
    headers: { "Content-Type": "application/json" }
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

        if (
            error.response?.status === 401 &&
            error.response?.data.detail === "invalid access token" &&
            !originalRequest.sent
        ) {
            originalRequest.sent = true
            try {
                const res = await refreshInstance.post("/auth/refresh-token")
                useTokenStore.getState().setAccessToken(res.data.accessToken)

                originalRequest.headers = originalRequest.headers || {}
                originalRequest.headers["Authorization"] = `Bearer ${res.data.accessToken}`

                return axiosInstance(originalRequest)
            } catch (refreshError) {
                useTokenStore.getState().removeAccessToken()
                return Promise.reject(refreshError)
            }
        }
        return Promise.reject(error)
    }
)

export default axiosInstance
