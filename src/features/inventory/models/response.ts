export type Ingredient = {
    id: string
    name: string
    baseUnit: string
    categoryId: string
}

// === INGREDIENT ITEM (lấy từ BE IngredientViewRes) ===
export type IngredientItem = {
    id: string
    name: string
    baseUnit: string
    categoryId: string | null
}

// === INGREDIENT CATEGORY (lấy từ BE IngreCateViewRes) ===
export type IngredientCategory = {
    id: string
    name: string
    description: string | null
}

export type GetIngredientsResult = {
    items: IngredientItem[]
    totalCount: number
    totalPages: number
}

export type StockItem = {
    storeId: string
    ingredientViewRes: Ingredient
    quantity: string
    minThreshold: string
    stockStatus: number // 0=low, 1=normal, 2=high
}

export type GetAllStockResult = {
    items: StockItem[]
    totalCount: number
    totalPages: number
}
