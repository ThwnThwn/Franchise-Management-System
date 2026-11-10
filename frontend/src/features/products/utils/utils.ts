import { ProductAttribute } from "../models/products/respone"

export const parseAttributes = (jsonString?: string | null): ProductAttribute[] => {
    if (!jsonString) return []

    try {
        const parsed = JSON.parse(jsonString)

        return Object.entries(parsed).map(([key, value]) => ({
            key: key.charAt(0).toUpperCase() + key.slice(1),
            originalKey: key,
            value: String(value)
        }))
    } catch (error) {
        console.error("Lỗi parse attributes:", error)
        return []
    }
}
