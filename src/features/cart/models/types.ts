export interface CartItem {
    id: string
    sku: string
    productName: string
    customerId: string
    quantity: number
    basePrice: number

    // productId: UUID
    // variantId: UUID
    // sizeName?: string
    // attributes?: string
    // imageUrl?: string
    // // storeId?: string
    // store?: StoreDetailDto
}
