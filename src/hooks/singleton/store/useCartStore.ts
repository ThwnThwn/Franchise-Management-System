import { create } from "zustand"
import { GetProductListItemDto, ProductSizeListItemDto } from "@/features/menu/models/schema/respone"

export type CartItem = {
    productId: string
    productName: string
    imageUrl: string
    sku: string // lấy sku và attribute
    sizeId: string
    sizeName: string
    basePrice: number
    quantity: number
}

type CartStore = {
    items: CartItem[] // cho lưu vào localstorage của itemss
    addItem: (product: GetProductListItemDto, size: ProductSizeListItemDto, quantity?: number) => void
    removeItem: (productId: string, sizeId: string) => void
}

const CART_STORAGE_KEY = "cart_items"

const getInitialItems = (): CartItem[] => {
    if (typeof window === "undefined") return []

    try {
        const raw = window.localStorage.getItem(CART_STORAGE_KEY)
        if (!raw) return []
        const parsed = JSON.parse(raw) as CartItem[]
        if (!Array.isArray(parsed)) return []
        return parsed
    } catch {
        return [] // nếu chưa có thì trả vễ rỗng
    }
}

export const useCartStore = create<CartStore>()((set) => ({
    items: getInitialItems(),

    addItem: (product, size, quantity = 1) => {
        set((state) => {
            const index = state.items.findIndex(
                (x) => x.productId === product.id && x.sizeId === size.id
            )

            const nextItems =
                index >= 0
                    ? state.items.map((item, idx) =>
                        idx === index ? { ...item, quantity: item.quantity + quantity } : item
                    )
                    : [
                        ...state.items,
                        {
                            productId: product.id as unknown as string,
                            productName: product.name,
                            imageUrl: product.imageUrl,
                            sku: size.sku,
                            sizeId: size.id as unknown as string,
                            sizeName: size.name ?? "",
                            basePrice: size.basePrice,
                            quantity
                        }
                    ]

            try {
                if (typeof window !== "undefined") {
                    window.localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(nextItems))
                }
            } catch {
                // nếu báo lối
            }

            return { items: nextItems }
        })
    },

    removeItem: (productId, sizeId) => {
        set((state) => {
            const nextItems = state.items.filter(
                (item) => !(item.productId === productId && item.sizeId === sizeId)
            )

            try {
                if (typeof window !== "undefined") {
                    window.localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(nextItems))
                }
            } catch {
                // nếu báo lỗi
            }

            return { items: nextItems }
        })
    }
}))
