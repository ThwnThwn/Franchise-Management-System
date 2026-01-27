import React from "react"
import { Divider, Image } from "@heroui/react"
import { Order } from "@/features/orders/models/response"
import { getPaymentMethodLabel } from "@/features/orders/utils"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import { ORDER_TEXT } from "@/features/orders/constants"

export const OrderDetail = ({ order }: { order: Order }) => {
    return (
        <>
            {/* Thông tin chung */}
            <div className="grid grid-cols-2 gap-4 bg-default-50 p-3 rounded-lg">
                <div>
                    <p className="text-xs text-default-500 uppercase">
                        {ORDER_TEXT.labels.CUSTOMER}
                    </p>
                    <p className="font-semibold">
                        {order.customerId ? ORDER_TEXT.labels.MEMBER : ORDER_TEXT.labels.GUEST}
                    </p>
                </div>
                <div>
                    <p className="text-xs text-default-500 uppercase">
                        {ORDER_TEXT.labels.PAYMENT_METHOD}
                    </p>
                    <p className="font-semibold">{getPaymentMethodLabel(order.paymentMethod)}</p>
                </div>
                {order.notes && (
                    <div className="col-span-2">
                        <p className="text-xs text-default-500 uppercase">
                            {ORDER_TEXT.labels.NOTES}
                        </p>
                        <p className="italic text-sm">{order.notes}</p>
                    </div>
                )}
            </div>

            <Divider className="my-2" />

            <h3 className="font-semibold text-lg">{ORDER_TEXT.labels.PRODUCT_LIST}</h3>

            <div className="flex flex-col gap-4">
                {order.items.map((item) => (
                    <div key={item.id} className="flex justify-between items-center gap-3">
                        <div className="flex gap-3 items-center">
                            <div className="w-16 h-16 bg-default-100 rounded-lg shrink-0 border border-default-200 overflow-hidden">
                                {item.imageUrl ? (
                                    <Image
                                        src={item.imageUrl}
                                        alt={item.productName}
                                        classNames={{
                                            wrapper: "w-full h-full",
                                            img: "object-cover w-full h-full"
                                        }}
                                    />
                                ) : (
                                    <div className="w-full h-full flex items-center justify-center text-xs text-default-400">
                                        {ORDER_TEXT.labels.NO_IMAGE}
                                    </div>
                                )}
                            </div>
                            <div className="flex flex-col">
                                <span className="font-medium text-sm line-clamp-1">
                                    {item.productName}
                                </span>
                                <span className="text-xs text-default-400">
                                    {ORDER_TEXT.labels.SKU} {item.sku}
                                </span>
                                <span className="text-xs text-default-500">
                                    {formatCurrencyWithSymbol(item.unitPrice)} x {item.quantity}
                                </span>
                            </div>
                        </div>
                        <div className="text-right font-semibold">
                            {formatCurrencyWithSymbol(item.unitPrice * item.quantity)}
                        </div>
                    </div>
                ))}
            </div>

            <Divider className="my-2" />

            <div className="space-y-2">
                <div className="flex justify-between text-sm">
                    <span className="text-default-500">{ORDER_TEXT.labels.SUBTOTAL}</span>
                    <span>{formatCurrencyWithSymbol(order.subtotal)}</span>
                </div>
                <div className="flex justify-between text-sm">
                    <span className="text-default-500">{ORDER_TEXT.labels.TAX}</span>
                    <span>{formatCurrencyWithSymbol(order.tax)}</span>
                </div>
                <Divider />
                <div className="flex justify-between text-lg font-bold text-primary">
                    <span>{ORDER_TEXT.labels.TOTAL}</span>
                    <span>{formatCurrencyWithSymbol(order.totalAmount)}</span>
                </div>
            </div>
        </>
    )
}
