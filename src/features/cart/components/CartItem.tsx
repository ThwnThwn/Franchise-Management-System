"use client"

import React from "react"
import { Card, CardBody } from "@heroui/react"
import { Minus, Plus, Trash2 } from "lucide-react"
import { CartItem as CartItemType } from "../models/types"
import { ButtonIconStyled, ImageStyled } from "@/components"

interface CartItemProps {
    item: CartItemType
    onUpdateQuantity: (sku: string, quantity: number) => void
    onRemove: (sku: string) => void
}

export const CartItem: React.FC<CartItemProps> = ({ item, onUpdateQuantity, onRemove }) => {
    const handleDecrease = () => {
        if (item.quantity > 1) {
            onUpdateQuantity(item.sku, item.quantity - 1)
        }
    }

    const handleIncrease = () => {
        onUpdateQuantity(item.sku, item.quantity + 1)
    }

    const itemTotal = item.basePrice * item.quantity

    return (
        <Card className="mb-3">
            <CardBody className="p-3">
                <div className="flex gap-3">
                    {/* Product Image */}
                    <ImageStyled
                        src={item.imageUrl}
                        alt={item.productName}
                        width={80}
                        height={80}
                    />

                    {/* Product Info */}
                    <div className="flex-1 min-w-0">
                        <h4 className="font-semibold text-sm truncate">{item.productName}</h4>
                        {item.sizeName && (
                            <p className="text-xs text-gray-500">Size: {item.sizeName}</p>
                        )}
                        {item.attributes && (
                            <p className="text-xs text-gray-500">{item.attributes}</p>
                        )}
                        <p className="text-sm font-medium text-primary mt-1">
                            {item.basePrice.toLocaleString("vi-VN")} đ
                        </p>

                        {/* Quantity Controls */}
                        <div className="flex items-center gap-2 mt-2">
                            <ButtonIconStyled size="sm" variant="flat" onPress={handleDecrease}>
                                <Minus size={14} />
                            </ButtonIconStyled>
                            <span className="text-sm font-medium w-8 text-center">
                                {item.quantity}
                            </span>
                            <ButtonIconStyled size="sm" variant="flat" onPress={handleIncrease}>
                                <Plus size={14} />
                            </ButtonIconStyled>
                        </div>
                    </div>

                    {/* Item Total & Remove */}
                    <div className="flex flex-col items-end justify-between">
                        <ButtonIconStyled
                            size="sm"
                            variant="light"
                            color="danger"
                            onPress={() => onRemove(item.sku)}
                        >
                            <Trash2 size={16} />
                        </ButtonIconStyled>
                        <p className="font-semibold text-sm">
                            {itemTotal.toLocaleString("vi-VN")} đ
                        </p>
                    </div>
                </div>
            </CardBody>
        </Card>
    )
}
