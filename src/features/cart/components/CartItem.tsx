import React from "react"
import { CardBody } from "@heroui/react"
import { Minus, Plus, Trash2 } from "lucide-react"
import { CartItem as CartItemType } from "../models/types"
import { ButtonIconStyled, CardStyled, ImageStyled } from "@/components"
import { parseAttributes } from "@/features/menu/utils/utils"

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
    const attrs = parseAttributes(item.attributes)

    return (
        <CardStyled className="mb-3">
            <CardBody>
                <div className="flex justify-between items-center gap-3 w-full">
                    {/* Product Info */}
                    <div className="flex gap-3">
                        <div className="w-16 h-16 bg-default-100 rounded-lg shrink-0 border border-default-200 overflow-hidden">
                            <ImageStyled
                                src={item.imageUrl}
                                alt={item.productName}
                                className="h-20 w-20"
                            />
                        </div>
                        <div className="flex-1 min-w-0 gap-1">
                            {/* Product Image */}

                            <h4 className="font-semibold text-sm truncate">{item.productName}</h4>
                            {/* {item.sizeName && (
                                <p className="text-xs text-gray-500">Size: {item.sizeName}</p>
                            )} */}

                            {item.attributes && (
                                <div className="flex gap-2">
                                    {attrs.map((attr) => (
                                        <span
                                            key={attr.originalKey}
                                            className="px-2 py-1 text-xs bg-gray-100 rounded text-gray-600"
                                        >
                                            {/* Hiển thị kiểu: Size: L */}
                                            <span className="font-semibold">{attr.key}:</span>{" "}
                                            {attr.value}
                                        </span>
                                    ))}
                                </div>
                            )}

                            <p className="text-sm font-medium text-primary mt-1">
                                {item.basePrice.toLocaleString("vi-VN")} đ
                            </p>
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
                {/* Quantity Controls */}
                <div className="flex justify-end items-center gap-2 mt-2">
                    <ButtonIconStyled size="sm" variant="flat" onPress={handleDecrease}>
                        <Minus size={14} />
                    </ButtonIconStyled>
                    <span className="text-sm font-medium w-8 text-center">{item.quantity}</span>
                    <ButtonIconStyled size="sm" variant="flat" onPress={handleIncrease}>
                        <Plus size={14} />
                    </ButtonIconStyled>
                </div>
            </CardBody>
        </CardStyled>
    )
}
