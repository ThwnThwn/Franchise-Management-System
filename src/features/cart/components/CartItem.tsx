"use client"

import React from "react"
import { Card, CardBody, Button } from "@heroui/react"
import { Minus, Plus, Trash2 } from "lucide-react"
import { CartItem as CartItemType } from "../models/types"
import Image from "next/image"

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
                    <div className="relative w-20 h-20 flex-shrink-0 rounded-lg overflow-hidden bg-gray-100">
                        {item.imageUrl ? (
                            <Image
                                src={item.imageUrl}
                                alt={item.productName}
                                fill
                                className="object-cover"
                            />
                        ) : (
                            <div className="w-full h-full flex items-center justify-center text-gray-400">
                                No Image
                            </div>
                        )}
                    </div>

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
                            <Button
                                isIconOnly
                                size="sm"
                                variant="flat"
                                onClick={handleDecrease}
                                className="min-w-8 h-8"
                            >
                                <Minus size={14} />
                            </Button>
                            <span className="text-sm font-medium w-8 text-center">
                                {item.quantity}
                            </span>
                            <Button
                                isIconOnly
                                size="sm"
                                variant="flat"
                                onClick={handleIncrease}
                                className="min-w-8 h-8"
                            >
                                <Plus size={14} />
                            </Button>
                        </div>
                    </div>

                    {/* Item Total & Remove */}
                    <div className="flex flex-col items-end justify-between">
                        <Button
                            isIconOnly
                            size="sm"
                            variant="light"
                            color="danger"
                            onClick={() => onRemove(item.sku)}
                            className="min-w-8 h-8"
                        >
                            <Trash2 size={16} />
                        </Button>
                        <p className="font-semibold text-sm">
                            {itemTotal.toLocaleString("vi-VN")} đ
                        </p>
                    </div>
                </div>
            </CardBody>
        </Card>
    )
}
