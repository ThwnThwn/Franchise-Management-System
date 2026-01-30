"use client"

import React from "react"
import { Badge } from "@heroui/react"
import { ShoppingCart } from "lucide-react"
import { ButtonIconStyled } from "@/components"
import { useCartStore } from "@/features/cart/store"

interface CartButtonProps {
    onPress: () => void
}

export const CartButton: React.FC<CartButtonProps> = ({ onPress }) => {
    const itemCount = useCartStore((state) => state.count)

    return (
        <Badge content={itemCount} color="primary" isInvisible={itemCount === 0} shape="circle">
            <ButtonIconStyled
                color="primary"
                onPress={onPress}
                className="relative"
                aria-label="Giỏ hàng"
            >
                <ShoppingCart size={15} />
            </ButtonIconStyled>
        </Badge>
    )
}
