import axios from "axios"
import { BACKEND_API_URL } from "@/constants/env"

const axiosServer = axios.create({
    baseURL: BACKEND_API_URL,
    withCredentials: true,
    headers: { "Content-Type": "application/json" }
})

export default axiosServer
