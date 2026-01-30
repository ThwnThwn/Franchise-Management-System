import { UUID } from "crypto"

export type Product = {
    id: UUID
    name: string
    description?: string
    imageUrl: string
    isActive: boolean
    categoryId: UUID
    categoryName: string
    sizes: ProductSize[]
}

export type ProductSize = {
    id: UUID
    sku: string
    name?: string
    attributes?: string
    basePrice: number
    cost: number
    isActive: boolean
}

export type GetProductsResultDto = {
    products: Product[]
    totalCount: number
}

export interface ProductAttribute {
    key: string
    value: string
    originalKey: string
}
