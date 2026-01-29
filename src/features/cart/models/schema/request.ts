import { UUID } from "crypto"

export interface AddToCartRequest {
    variantId: UUID
    quantity: number
    storeId?: string
    sku?: string
}

export interface UpdateCartItemRequest {
    sku: string
    quantity: number
}

export interface DeleteListCartItemRequest {
    skus: string[]
}
