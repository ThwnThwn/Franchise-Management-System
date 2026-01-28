import { create } from "zustand"
import { persist } from "zustand/middleware"
import { CartItem } from "@/features/cart/models/types"

export interface CartState {
    items: CartItem[]
    total: number
    count: number

    addItem: (item: Omit<CartItem, "quantity"> & { quantity?: number }) => void
    removeItem: (sku: string) => void
    updateQuantity: (sku: string, quantity: number) => void
    clearCart: () => void
}

const calculateSummary = (items: CartItem[]) => {
    const total = items.reduce((sum, item) => sum + item.basePrice * item.quantity, 0)
    const count = items.reduce((sum, item) => sum + item.quantity, 0)
    return { total, count }
}

export const useCartStore = create<CartState>()(
    persist(
        (set, get) => ({
            items: [],
            total: 0,
            count: 0,

            addItem: (item) => {
                const currentItems = get().items
                const existingItemIndex = currentItems.findIndex((i) => i.sku === item.sku)

                let newItems: CartItem[]

                if (existingItemIndex > -1) {
                    newItems = currentItems.map((i, index) =>
                        index === existingItemIndex
                            ? { ...i, quantity: i.quantity + (item.quantity || 1) }
                            : i
                    )
                } else {
                    newItems = [...currentItems, { ...item, quantity: item.quantity || 1 }]
                }

                set({
                    items: newItems,
                    ...calculateSummary(newItems)
                })
            },

            removeItem: (sku) => {
                const newItems = get().items.filter((item) => item.sku !== sku)
                set({
                    items: newItems,
                    ...calculateSummary(newItems)
                })
            },

            updateQuantity: (sku, quantity) => {
                let newItems = get().items

                if (quantity <= 0) {
                    newItems = newItems.filter((item) => item.sku !== sku)
                } else {
                    newItems = newItems.map((item) =>
                        item.sku === sku ? { ...item, quantity } : item
                    )
                }

                set({
                    items: newItems,
                    ...calculateSummary(newItems)
                })
            },

            clearCart: () => {
                set({
                    items: [],
                    total: 0,
                    count: 0
                })
            }
        }),
        {
            name: "cart-storage"
        }
    )
)
