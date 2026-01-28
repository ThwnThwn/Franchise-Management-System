import { UUID } from "crypto"

export interface CartItem {
    sku: string
    productId: UUID
    variantId: UUID
    productName: string
    sizeName?: string
    attributes?: string
    basePrice: number
    quantity: number
    imageUrl?: string
}
