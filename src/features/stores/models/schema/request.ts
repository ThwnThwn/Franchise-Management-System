export type GetStoresParams = {
    page?: number
    pageSize?: number
    keyword?: string
}

export type FindNearbyStoresParams = {
    userLat: number
    userLong: number
    radiusKm?: number
    limit?: number
}
