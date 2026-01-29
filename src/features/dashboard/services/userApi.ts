import { User } from "@/models/user"
import axiosInstance from "@/utils/axios"
import { buildQueryParams, requestWrapper } from "@/utils/helpers/axiosHelper"
import { CreateUserRequest, UserFilterParams } from "../models/user/request"
import { RoleSlug } from "@/constants/enums"
import { PageResult } from "@/models/common/respone"
import { PaginationParams } from "@/models/common/request"

export const userApi = {
    getAll: ({ query, pagination }: { query: UserFilterParams; pagination: PaginationParams }) =>
        requestWrapper<PageResult<User>>(async () => {
            const params = {
                ...buildQueryParams(query),
                ...buildQueryParams(pagination)
            }
            const res = await axiosInstance.get("/users", { params })
            return res.data
        }),

    getById: (id: string) =>
        requestWrapper<User>(async () => {
            const res = await axiosInstance.get(`/users/${id}`)
            return res.data
        }),

    create: (req: CreateUserRequest) =>
        requestWrapper<User>(async () => {
            const res = await axiosInstance.post("/users", req)
            return res.data
        }),

    updateRole: ({ id, roleSlug }: { id: string; roleSlug: RoleSlug }) =>
        requestWrapper<User>(async () => {
            const res = await axiosInstance.patch(`/users/${id}/role`, { roleSlug })
            return res.data
        })
}
