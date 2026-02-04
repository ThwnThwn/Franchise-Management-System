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
}

export type AdjustStockRequest = {
    storeId: string
    items: IngredientStockItem[]
}
