import { requestWrapper } from "@/utils/helpers/axiosHelper"
import { RolePermissionGroupRes, RoleResponse } from "../models/respone"
import axiosInstance from "@/utils/axios"
import {
    CreateRoleRequest,
    UpdateRolePermissionsRequest,
    UpdateRoleRequest
} from "../models/request"

export const roleApi = {
    getAll: () =>
        requestWrapper<RoleResponse[]>(async () => {
            const res = await axiosInstance.get("/roles")
            return res.data
        }),

    create: (req: CreateRoleRequest) =>
        requestWrapper<void>(async () => {
            await axiosInstance.post("/roles", req)
        }),

    update: (id: string, req: UpdateRoleRequest) =>
        requestWrapper<void>(async () => {
            await axiosInstance.patch(`/roles/${id}`, req)
        }),

    getPermissionsByRoleId: (id: string) =>
        requestWrapper<RolePermissionGroupRes[]>(async () => {
            const res = await axiosInstance.get(`/roles/${id}/permissions`)
            return res.data
        }),

    updateRolePermissions: (id: string, req: UpdateRolePermissionsRequest) =>
        requestWrapper<void>(async () => {
            await axiosInstance.put(`/roles/${id}/permissions`, req)
        })
}
