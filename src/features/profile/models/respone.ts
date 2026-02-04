import { RoleUserResponse } from "@/features/role-permissions/models/respone"

export type UserProfileViewRes = {
    id: string
    email?: string
    userName?: string
    firstName: string
    lastName?: string
    address?: string
    gender?: number
    dateOfBirth?: string
    phone?: string
    role?: RoleUserResponse
}
