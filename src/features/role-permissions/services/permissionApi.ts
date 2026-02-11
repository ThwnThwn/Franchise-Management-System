import axiosInstance from "@/utils/axios"
import { PermissionGroup } from "../models/respone"
import { UpdatePermissionInfoRequest } from "../models/request"

/**
 * PERMISSION API
 * Quản lý phân quyền hệ thống.
 */
export const permissionApi = {
    // Lấy toàn bộ nhóm quyền
    getAll: async (): Promise<PermissionGroup[]> => {
        const res = await axiosInstance.get("/permissions")
        return res.data
    },

    // Cập nhật thông tin quyền
    update: async (id: string, req: UpdatePermissionInfoRequest): Promise<void> => {
        await axiosInstance.patch(`/permissions/${id}`, req)
    }
}
