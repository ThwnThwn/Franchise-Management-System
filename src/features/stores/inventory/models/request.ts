// === INGREDIENT PARAMS (matches BE GetAllIngredientReq) ===
export type GetIngredientsParams = {
    categoryId?: string
    page?: number
    pageSize?: number
    keyword?: string
}

// === INGREDIENT CRUD REQUESTS (matches BE CreateIngredientReq / UpdateIngredientReq) ===
export type CreateIngredientRequest = {
    name: string
    baseUnit: string
    categoryId?: string | null
}

export type UpdateIngredientRequest = {
    name?: string
    baseUnit?: string
    categoryId?: string | null
}

// === ADD STOCK REQUEST (matches BE IngredientStockItem -> PUT /stores/:id/stock) ===
export type AddIngredientStockItem = {
    ingredientId: string
    quantity: number
    minThreshold?: number // optional - ngưỡng tối thiểu
}

export type AddIngredientToStoreRequest = {
    storeId: string
    items: AddIngredientStockItem[]
    // Extra metadata for UI only (not sent to BE)
    supplierInfo?: string
    expirationDate?: string
    reason?: string
}

export type GetAllStockParams = {
    storeIds?: string[]
    categoryIds?: string[]
    stockStatus?: number
    ingredientIds?: string[]
    page?: number
    pageSize?: number
}

export type IngredientStockItem = {
    ingredientId: string
    quantity: number
    minThreshold?: number
}

export type AdjustStockRequest = {
    storeId: string
    items: IngredientStockItem[]
}
