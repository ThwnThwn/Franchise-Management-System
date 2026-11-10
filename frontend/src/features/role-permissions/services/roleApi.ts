import axiosInstance from "@/utils/axios"
import {
    CreateRoleRequest,
    UpdateRolePermissionsRequest,
    UpdateRoleRequest
} from "../models/request"
import { PermissionGroup, RoleResponse } from "../models/respone"

/**
 * ROLE API
 * Quản lý vai trò và phân quyền theo vai trò.
 */
export const roleApi = {
    // Lấy danh sách tất cả vai trò
    getAll: async (): Promise<RoleResponse[]> => {
        const res = await axiosInstance.get("/roles")
        return res.data
    },

    // Tạo vai trò mới
    create: async (req: CreateRoleRequest): Promise<void> => {
        await axiosInstance.post("/roles", req)
    },

    // Cập nhật thông tin cơ bản của vai trò
    update: async (id: string, req: UpdateRoleRequest): Promise<void> => {
        await axiosInstance.patch(`/roles/${id}`, req)
    },

    // Lấy danh sách quyền hiện tại của một vai trò
    getPermissionsByRoleId: async (id: string): Promise<PermissionGroup[]> => {
        const res = await axiosInstance.get(`/roles/${id}/permissions`)
        return res.data
    },

    // Cập nhật (ghi đè) danh sách quyền cho vai trò
    updateRolePermissions: async (id: string, req: UpdateRolePermissionsRequest): Promise<void> => {
        await axiosInstance.put(`/roles/${id}/permissions`, req)
    }
}
