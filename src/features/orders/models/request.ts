export interface CartItemCreateRequest {
    sku: string
    quantity: number
}

export interface CreateOrderRequest {
    items: CartItemCreateRequest[]
    // tax: number
    // paymentMethod: PaymentMethod
    notes: string
    storeId?: string
}
