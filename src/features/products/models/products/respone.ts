import { UUID } from "crypto"

export type Product = {
    id: UUID
    name: string
    description?: string
    imageUrl: string
    isActive: boolean
    categoryId: UUID
    categoryName: string
    variants: ProductVariant[]
}

export type ProductVariant = {
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

export interface ProductVariantDetail {
    productId: string
    variantId: string
    productName: string
    sku: string
    attributes: string
    basePrice: number
    imageUrl: string
}
