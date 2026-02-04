import { requestWrapper } from "@/utils/helpers/axiosHelper"
import { PermissionGroup } from "../models/respone"
import axiosInstance from "@/utils/axios"
import { UpdatePermissionInfoRequest } from "../models/request"

export const permissionApi = {
    getAll: () =>
        requestWrapper<PermissionGroup[]>(async () => {
            const res = await axiosInstance.get("/permissions")
            return res.data
        }),

    update: (id: string, req: UpdatePermissionInfoRequest) =>
        requestWrapper<void>(async () => {
            await axiosInstance.patch(`/permissions/${id}`, req)
        })
}
