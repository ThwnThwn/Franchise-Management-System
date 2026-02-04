export type Ingredient = {
    id: string
    name: string
    baseUnit: string
    categoryId: string
}

export type StockItem = {
    storeId: string
    ingredient: Ingredient
    quantity: string
    minThreshold: string
    stockStatus: number // 0=low, 1=normal, 2=high
}

export type GetAllStockResult = {
    stocks: StockItem[]
    totalCount: number
    totalPages: number
}
