import { User } from "@/models/user"
import axiosInstance from "@/utils/axios"
import { buildQueryParams } from "@/utils/helpers/axiosHelper"
import { CreateUserRequest, UpdateUserRequest, UserFilterParams } from "../models/request"
import { RoleSlug } from "@/constants/enums"
import { PageResult } from "@/models/common/respone"
import { PaginationParams } from "@/models/common/request"

/**
 * USER API
 */
export const userApi = {
    // Lấy danh sách người dùng có phân trang và filter
    getAll: async ({
        query,
        pagination
    }: {
        query: UserFilterParams
        pagination: PaginationParams
    }): Promise<PageResult<User>> => {
        const params = {
            ...buildQueryParams(query),
            ...buildQueryParams(pagination)
        }
        const res = await axiosInstance.get("/users", { params })
        return res.data
    },

    // Lấy chi tiết một người dùng
    getById: async (id: string): Promise<User> => {
        const res = await axiosInstance.get(`/users/${id}`)
        return res.data
    },

    // Tạo người dùng mới (thường dùng cho Admin/Manager tạo nhân viên)
    create: async (req: CreateUserRequest): Promise<User> => {
        const res = await axiosInstance.post("/users", req)
        return res.data
    },

    // Tạo người dùng mới (thường dùng cho Admin/Manager tạo nhân viên)
    update: async ({ id, req }: { id: string; req: UpdateUserRequest }): Promise<User> => {
        const res = await axiosInstance.patch(`/users/${id}`, req)
        return res.data
    },

    // Cập nhật vai trò nhân viên
    updateRole: async ({ id, roleSlug }: { id: string; roleSlug: RoleSlug }): Promise<User> => {
        const res = await axiosInstance.patch(`/users/${id}/role`, { roleSlug })
        return res.data
    }
}
