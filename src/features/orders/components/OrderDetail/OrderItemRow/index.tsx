"use client"

import React from "react"
import { ImageStyled } from "@/components"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import { OrderItem } from "@/features/orders/models/response"

interface OrderItemRowProps {
    item: OrderItem
}

export const OrderItemRow = ({ item }: OrderItemRowProps) => {
    return (
        <div className="grid grid-cols-12 items-center w-full text-left">
            {/* Cột Sản phẩm */}
            <div className="col-span-6 flex gap-4 items-center">
                <div className="w-14 h-14 bg-default-100 rounded-xl shrink-0 border border-default-200 overflow-hidden">
                    <ImageStyled
                        src={item.imageUrl}
                        alt={item.productName}
                        className="h-full w-full object-cover"
                    />
                </div>
                <div className="flex flex-col">
                    <span className="font-bold text-sm text-slate-800 line-clamp-1">
                        {item.productName}
                    </span>
                    <span className="text-[10px] font-mono text-default-400 uppercase tracking-tighter">
                        {item.sku}
                    </span>
                </div>
            </div>

            {/* Cột Đơn giá */}
            <div className="col-span-2 text-center text-sm font-medium text-slate-600">
                {formatCurrencyWithSymbol(item.unitPrice)}
            </div>

            {/* Cột Số lượng */}
            <div className="col-span-2 text-center text-sm font-bold text-slate-800">
                {item.quantity}
            </div>

            {/* Cột Thành tiền */}
            <div className="col-span-2 text-right font-black text-sm text-slate-900 pr-2">
                {formatCurrencyWithSymbol(item.unitPrice * item.quantity)}
            </div>
        </div>
    )
}
