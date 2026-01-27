import { RoleUserResponse } from "@/models/role"

export interface User {
    id: string
    email: string
    username: string
    phone: string
    address: string
    isBlocked: boolean
    firstName: string
    lastName: string
    gender: number
    dateOfBirth: string | undefined
    role: RoleUserResponse | undefined
    createdAt: string | undefined
    updatedAt: string | undefined
}
