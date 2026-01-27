import { useCartStore } from "../store/cartStore"

export const useCart = () => {
    const items = useCartStore((state) => state.items)
    const addItem = useCartStore((state) => state.addItem)
    const removeItem = useCartStore((state) => state.removeItem)
    const updateQuantity = useCartStore((state) => state.updateQuantity)
    const clearCart = useCartStore((state) => state.clearCart)
    const getCartTotal = useCartStore((state) => state.getCartTotal)
    const getCartItemCount = useCartStore((state) => state.getCartItemCount)

    const total = getCartTotal()
    const itemCount = getCartItemCount()

    return {
        items,
        addItem,
        removeItem,
        updateQuantity,
        clearCart,
        total,
        itemCount
    }
}
