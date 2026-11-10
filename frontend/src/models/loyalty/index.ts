export interface MembershipTier {
    id: string
    name: string
    minSpendRequired: string
    discountRate: number
    amountPerPoint: string
    valuePerPoint: string
}

export interface LoyaltyAccountResponse {
    id: string
    availablePoints: string
    totalSpent: string
    currentTier: MembershipTier
    userName: string
}
