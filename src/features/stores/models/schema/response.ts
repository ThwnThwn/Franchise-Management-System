export type StoreDetailDto = {
    id: string
    name: string
    address: string
    phone?: string
    imageUrl?: string
    latitude?: number
    longitude?: number
    isOpenNow: boolean
}

export interface ActiveStaffDto {
    id: string
    firstName: string
    lastName: string
    email: string
    avatarUrl: string
}
