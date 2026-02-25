export interface CartItem {
    id: string
    sku: string
    productName: string
    imageUrl?: string
    attributes?: string
    customerId: string
    quantity: number
    basePrice: number

    // productId: UUID
    // variantId: UUID
    // sizeName?: string
    // // storeId?: string
    // store?: StoreDetailDto
}
