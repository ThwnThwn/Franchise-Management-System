export type StoreDetailDto = {
    id: string
    name: string
    address: string
    phone: string
    imageUrl: string
    latitude: number
    longitude: number
    isOpenNow: boolean
}

export type GetStoresResponseDto = {
    items: StoreDetailDto[]
    totalCount: number
    totalPages: number
}
