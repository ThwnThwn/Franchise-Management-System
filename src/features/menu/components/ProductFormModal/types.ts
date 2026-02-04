import { ProductAttribute } from "../../models/products/respone"

export interface AttributeUI {
    key: string
    value: string
}

export interface VariantUI {
    sku: string
    name: string
    basePrice: number
    cost: number
    isActive: boolean
    attributes: ProductAttribute[]
}

export interface ProductFormValues {
    name: string
    description: string
    categoryId: string
    isActive: boolean
    variants: VariantUI[]
}
