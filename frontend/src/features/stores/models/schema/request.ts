export type GetStoresParams = {
    keyword?: string
}

export type FindNearbyStoresParams = {
    userLat: number
    userLong: number
    radiusKm?: number
    limit?: number
}
