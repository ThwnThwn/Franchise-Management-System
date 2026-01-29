import { RoleSlug } from "@/constants/enums"

export interface CreateUserRequest {
    username: string
    email: string
    phone: string
    address: string
    firstName: string
    lastName: string
    gender: number
    dateOfBirth: string
    roleSlug: RoleSlug
}

export interface UpdateUserRequest {
    username: string
    email: string
    phone: string
    address: string
    firstName: string
    lastName: string
    gender: number
    dateOfBirth: string
    isBlocked: boolean
    roleSlug: RoleSlug
}

export interface UpdateUserRoleRequest {
    id: string
    roleSlug: RoleSlug
}

export type UserFilterParams = {
    roleSlug?: RoleSlug
    keyword?: string
}
