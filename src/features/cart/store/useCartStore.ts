import { create } from "zustand"
import { persist, createJSONStorage } from "zustand/middleware"
import { CartItem } from "@/features/cart/models/types"
import { StoreDetailDto } from "@/features/stores"

export interface CartState {
    items: CartItem[]
    total: number
    count: number
    // currentStoreId: string | null
    currentStore: StoreDetailDto | null

    addItem: (item: Omit<CartItem, "quantity"> & { quantity?: number }) => void
    removeItem: (sku: string) => void
    updateQuantity: (sku: string, quantity: number) => void
    clearCart: () => void
    // setStoreId: (id: string | null) => void
    setStore: (store: StoreDetailDto | null) => void
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
            // currentStoreId: null,
            currentStore: null,

            // setStoreId: (id) => set({ currentStoreId: id }),
            setStore: (store) => {
                const currentStore = get().currentStore
                const currentItems = get().items

                if (
                    store?.id &&
                    currentStore?.id &&
                    store.id !== currentStore.id &&
                    currentItems.length > 0
                ) {
                    set({ items: [], total: 0, count: 0 })
                }

                set({ currentStore: store })
            },

            addItem: (item) => {
                const currentItems = get().items

                if (
                    currentItems.length > 0 &&
                    item.store &&
                    currentItems[0].store &&
                    item.store !== currentItems[0].store
                ) {
                    set({ items: [], total: 0, count: 0 })
                }

                if (item.store) {
                    set({ currentStore: item.store })
                }

                const freshItems = get().items
                const existingItemIndex = freshItems.findIndex((i) => i.sku === item.sku)

                let newItems: CartItem[]

                if (existingItemIndex > -1) {
                    newItems = freshItems.map((i, index) =>
                        index === existingItemIndex
                            ? { ...i, quantity: i.quantity + (item.quantity || 1) }
                            : i
                    )
                } else {
                    newItems = [...freshItems, { ...item, quantity: item.quantity || 1 }]
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
            name: "cart-storage",
            storage: createJSONStorage(() => localStorage),

            partialize: (state) => ({
                items: state.items,
                total: state.total,
                count: state.count,
                currentStore: state.currentStore
            })
        }
    )
)
