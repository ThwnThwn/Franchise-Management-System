"use client"

import React from "react"
import { Divider } from "@heroui/react"
import { Order } from "@/features/orders/models/response"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import { ORDER_TEXT, PaymentMethodLabels } from "@/features/orders/constants"
import { ImageStyled } from "@/components"

export const OrderDetail = ({ order }: { order: Order }) => {
    return (
        <div className="space-y-8">
            {/* 1. Thông tin khách hàng & Thanh toán - Bố cục ngang thoáng đạt */}
            <div className="flex flex-wrap justify-between gap-6 p-6 rounded-2xl border border-default-100 bg-default-50/30">
                <div className="space-y-1">
                    <p className="text-[10px] uppercase tracking-tighter text-default-400 font-bold">
                        {ORDER_TEXT.labels.CUSTOMER}
                    </p>
                    <p className="font-bold text-slate-800">
                        {order.customerId ? ORDER_TEXT.labels.MEMBER : ORDER_TEXT.labels.GUEST}
                    </p>
                </div>
                <div className="space-y-1">
                    <p className="text-[10px] uppercase tracking-tighter text-default-400 font-bold">
                        Phương thức thanh toán
                    </p>
                    <p className="font-bold text-slate-800">
                        {order.paymentMethod !== undefined
                            ? PaymentMethodLabels[order.paymentMethod]
                            : "-"}
                    </p>
                </div>
                {order.notes && (
                    <div className="space-y-1 min-w-50">
                        <p className="text-[10px] uppercase tracking-tighter text-default-400 font-bold">
                            {ORDER_TEXT.labels.NOTES}
                        </p>
                        <p className="italic text-xs text-default-600 line-clamp-2">
                            {order.notes}
                        </p>
                    </div>
                )}
            </div>

            {/* 2. Danh sách sản phẩm dạng bảng (Table-like Grid) */}
            <div className="space-y-4">
                {/* Header của bảng */}
                <div className="grid grid-cols-12 px-2 pb-2">
                    <div className="col-span-6 text-[10px] uppercase font-black text-default-400">
                        Sản phẩm
                    </div>
                    <div className="col-span-2 text-[10px] uppercase font-black text-default-400 text-center">
                        Đơn giá
                    </div>
                    <div className="col-span-2 text-[10px] uppercase font-black text-default-400 text-center">
                        Số lượng
                    </div>
                    <div className="col-span-2 text-[10px] uppercase font-black text-default-400 text-right">
                        Thành tiền
                    </div>
                </div>

                <Divider />

                <div className="flex flex-col gap-6 pt-2">
                    {order.items.map((item) => (
                        <div key={item.id} className="grid grid-cols-12 items-center px-2 group">
                            {/* Cột Sản phẩm */}
                            <div className="col-span-6 flex gap-4 items-center">
                                <div className="w-14 h-14 bg-default-100 rounded-xl shrink-0 border border-default-200 overflow-hidden group-hover:scale-105 transition-transform">
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
                            <div className="col-span-2 text-right font-black text-sm text-slate-900">
                                {formatCurrencyWithSymbol(item.unitPrice * item.quantity)}
                            </div>
                        </div>
                    ))}
                </div>
            </div>

            {/* 3. Phần tổng kết (Summary) - Căn lề phải cực đẹp */}
            <div className="min-w-full pt-4">
                <div className="w-full space-y-3 bg-default-50/50 p-6 rounded-3xl border border-default-100">
                    <div className="flex justify-between text-xs font-medium">
                        <span className="text-default-400 uppercase tracking-widest">
                            {ORDER_TEXT.labels.SUBTOTAL}
                        </span>
                        <span className="text-slate-700">
                            {formatCurrencyWithSymbol(order.subtotal)}
                        </span>
                    </div>
                    <div className="flex justify-between text-xs font-medium">
                        <span className="text-default-400 uppercase tracking-widest">
                            {ORDER_TEXT.labels.TAX} (10%)
                        </span>
                        <span className="text-slate-700">
                            {formatCurrencyWithSymbol(order.tax)}
                        </span>
                    </div>

                    <div className="pt-2">
                        <div className="border-t border-dashed border-default-200 my-2" />
                        <div className="flex justify-between items-center">
                            <span className="text-sm font-black uppercase tracking-tighter text-primary">
                                Tổng cộng
                            </span>
                            <span className="text-xl font-black text-primary">
                                {formatCurrencyWithSymbol(order.totalAmount)}
                            </span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    )
}
