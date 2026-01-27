import { create } from "zustand"
import { persist } from "zustand/middleware"
import { CartItem, CartState } from "../models/types"

export const useCartStore = create<CartState>()(
    persist(
        (set, get) => ({
            items: [],

            addItem: (item) => {
                const existingItemIndex = get().items.findIndex((i) => i.sku === item.sku)

                if (existingItemIndex > -1) {
                    // Item already exists, increase quantity
                    set((state) => ({
                        items: state.items.map((i, index) =>
                            index === existingItemIndex
                                ? { ...i, quantity: i.quantity + (item.quantity || 1) }
                                : i
                        )
                    }))
                } else {
                    // Add new item
                    set((state) => ({
                        items: [...state.items, { ...item, quantity: item.quantity || 1 }]
                    }))
                }
            },

            removeItem: (sku) => {
                set((state) => ({
                    items: state.items.filter((item) => item.sku !== sku)
                }))
            },

            updateQuantity: (sku, quantity) => {
                if (quantity <= 0) {
                    get().removeItem(sku)
                    return
                }

                set((state) => ({
                    items: state.items.map((item) =>
                        item.sku === sku ? { ...item, quantity } : item
                    )
                }))
            },

            clearCart: () => {
                set({ items: [] })
            },

            getCartTotal: () => {
                return get().items.reduce((total, item) => total + item.basePrice * item.quantity, 0)
            },

            getCartItemCount: () => {
                return get().items.reduce((count, item) => count + item.quantity, 0)
            }
        }),
        {
            name: "cart-storage"
        }
    )
)
