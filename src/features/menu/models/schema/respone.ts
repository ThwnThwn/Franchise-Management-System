import { UUID } from "crypto"

export type GetProductListItemDto = {
    id: UUID
    name: string
    description?: string
    imageUrl: string
    isActive: boolean
    categoryId: UUID
    sizes: ProductSizeListItemDto[]
}

export type getProductsResultDto = {
    Products: GetProductListItemDto[]
    TotalCount: number
}

export type ProductSizeListItemDto = {
    id: UUID
    sku: string
    name?: string
    attributes?: string
    basePrice: number
    isActive: boolean
}
