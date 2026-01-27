"use client"

import React from "react"
import { Button, Badge } from "@heroui/react"
import { ShoppingCart } from "lucide-react"
import { useCart } from "../hooks/useCart"

interface CartButtonProps {
    onClick: () => void
}

export const CartButton: React.FC<CartButtonProps> = ({ onClick }) => {
    const { itemCount } = useCart()

    return (
        <Badge content={itemCount} color="primary" isInvisible={itemCount === 0} shape="circle">
            <Button
                isIconOnly
                variant="flat"
                color="primary"
                onClick={onClick}
                className="relative"
                aria-label="Giỏ hàng"
            >
                <ShoppingCart size={20} />
            </Button>
        </Badge>
    )
}
