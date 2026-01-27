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

export interface CartState {
    items: CartItem[]
    addItem: (item: Omit<CartItem, "quantity"> & { quantity?: number }) => void
    removeItem: (sku: string) => void
    updateQuantity: (sku: string, quantity: number) => void
    clearCart: () => void
    getCartTotal: () => number
    getCartItemCount: () => number
}
