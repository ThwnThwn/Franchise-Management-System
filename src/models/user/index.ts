import { RoleUserResponse } from "@/models/role"

export interface User {
    id: string
    email: string
    username: string
    phone: string
    address: string
    firstName: string
    lastName: string
    gender: number
    dateOfBirth: string | undefined
    homeStoreId: string | undefined
    storeName: string | undefined
    isBlocked: boolean
    createdAt: string | undefined
    updatedAt: string | undefined
    role: RoleUserResponse | undefined
}
