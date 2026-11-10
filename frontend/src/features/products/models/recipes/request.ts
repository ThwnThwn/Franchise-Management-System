export interface GetRecipesReq {
    variantIds: string[]
}

/** Payload cho từng dòng nguyên liệu khi update công thức của một variant */
export interface UpdateRecipeItem {
    ingredientId: string
    quantity: number
}
