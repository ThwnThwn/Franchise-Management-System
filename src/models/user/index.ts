import { Gender } from "@/constants/enums"
import { RoleUserResponse } from "@/models/role"

export interface MembershipTier {
    id: string
    name: string
    minSpendRequired: number
    discountRate: number
    amountPerPoint: number
    valuePerPoint: number
}

export interface LoyaltyResponse {
    id: string
    availablePoints: number
    totalSpent: number
    currentTier: MembershipTier
}

export interface User {
    id: string
    username: string
    email: string
    phone: string
    address: string
    firstName: string
    lastName: string
    gender: Gender
    dateOfBirth?: string

    avatarUrl?: string
    isBlocked: boolean

    homeStoreId?: string
    currentHourlyWage?: number
    role?: RoleUserResponse

    loyalty?: LoyaltyResponse

    createdAt?: string
    updatedAt?: string
}
