"use client"

import React from "react"
import { Badge } from "@heroui/react"
import { ShoppingCart } from "lucide-react"
import { useCart } from "../hooks/useCart"
import { ButtonIconStyled } from "@/components"

interface CartButtonProps {
    onPress: () => void
}

export const CartButton: React.FC<CartButtonProps> = ({ onPress }) => {
    const { itemCount } = useCart()

    return (
        <Badge content={itemCount} color="primary" isInvisible={itemCount === 0} shape="circle">
            <ButtonIconStyled
                variant="flat"
                color="primary"
                onPress={onPress}
                className="relative"
                aria-label="Giỏ hàng"
            >
                <ShoppingCart size={20} />
            </ButtonIconStyled>
        </Badge>
    )
}
