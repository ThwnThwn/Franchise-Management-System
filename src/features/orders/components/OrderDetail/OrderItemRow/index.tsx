"use client"

import React from "react"
import { ImageStyled } from "@/components"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import { OrderItem, OrderSaleItem } from "@/features/orders/models/response"

interface OrderItemRowProps {
    item: OrderItem
    sale?: OrderSaleItem
    isGift?: boolean
    overrideQuantity?: number
}

export const OrderItemRow = ({ item, sale, isGift = false }: OrderItemRowProps) => {
    const displayQuantity = sale ? item.quantity - sale.amount : item.quantity
    const displayTotal = isGift ? 0 : item.unitPrice * displayQuantity

    if (displayQuantity <= 0) return null

    return (
        <div
            className={`grid grid-cols-12 items-center w-full text-left ${isGift ? "opacity-90" : ""}`}
        >
            {/* Cột Sản phẩm */}
            <div className="col-span-6 flex gap-4 items-center">
                <div
                    className={`w-14 h-14 rounded-xl shrink-0 border overflow-hidden relative ${isGift ? "bg-success/10 border-success/20" : "bg-default-100 border-default-200"}`}
                >
                    <ImageStyled
                        src={item.imageUrl}
                        alt={item.productName}
                        className={`h-full w-full object-cover ${isGift ? "grayscale-30" : ""}`}
                    />
                    {isGift && (
                        <div className="absolute top-0 right-0 bg-success text-white text-[8px] font-bold px-1 rounded-bl-lg">
                            Quà
                        </div>
                    )}
                </div>
                <div className="flex flex-col">
                    <span
                        className={`font-bold text-sm line-clamp-1 ${isGift ? "text-success" : "text-slate-800"}`}
                    >
                        {item.productName} {isGift ? "(Quà tặng)" : ""}
                    </span>
                    <span
                        className={`text-[10px] font-mono uppercase tracking-tighter ${isGift ? "text-success/70" : "text-default-400"}`}
                    >
                        {item.sku}
                    </span>
                </div>
            </div>

            {/* Cột Đơn giá */}
            <div
                className={`col-span-2 text-center text-sm font-medium ${isGift ? "text-slate-500 line-through" : "text-slate-600"}`}
            >
                {formatCurrencyWithSymbol(item.unitPrice)}
            </div>

            {/* Cột Số lượng */}
            <div
                className={`col-span-2 text-center text-sm font-bold ${isGift ? "text-success" : "text-slate-800"}`}
            >
                {displayQuantity}
            </div>

            {/* Cột Thành tiền */}
            <div
                className={`col-span-2 text-right font-black text-sm pr-2 ${isGift ? "text-success" : "text-slate-900"}`}
            >
                {isGift ? "0 ₫" : formatCurrencyWithSymbol(displayTotal)}
            </div>
        </div>
    )
}
