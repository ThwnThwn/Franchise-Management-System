import { User } from "@/models/user"
import axiosInstance from "@/utils/axios"
import { requestWrapper } from "@/utils/helpers/axiosHelper"
import { CreateUserRequest } from "../models/user/request"
import { RoleSlug } from "@/constants/enums"

export const userApi = {
    getAll: () =>
        requestWrapper<User[]>(async () => {
            const res = await axiosInstance.get("/users")
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
