export type GetAllProductParams = {
    page?: number
    pageSize?: number
    name?: string
    // segmentId?: string
    storeId?: string
    keyword?: string
}

export type CreateProductRequest = {
    name: string
    description: string
    categoryId: string
    variants: CreateProductVariant[]
}

export type CreateProductVariant = {
    sku: string
    name: string
    attributes: string
    basePrice: number
    cost: number
    isActive: boolean
}

export type UpdateProductRequest = {
    name?: string
    description?: string
    imageUrl?: string
    categoryId?: string
    variants?: UpdateProductVariant[]
    isActive?: boolean
}

export type UpdateProductVariant = {
    sku?: string
    name?: string
    attributes?: string
    basePrice?: number
    cost?: number
    isActive?: boolean
}
