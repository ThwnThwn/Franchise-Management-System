import React from "react"
import { CardHeader, CardBody, CardFooter, Chip, Divider } from "@heroui/react"
import { Order } from "@/features/orders/models/response"
import { getOrderStatusColor, getOrderStatusLabel } from "@/features/orders/"
import { Calendar } from "lucide-react"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import { CardStyled } from "@/components"
import { ORDER_TEXT } from "@/features/orders/constants"
import { useDay } from "@/hooks"
import { DEFAULT_DATE_FORMAT } from "@/constants/constants"

interface OrderCardProps {
    order: Order
    onClick: (order: Order) => void
    isLatest?: boolean
}

export const OrderCard = ({ order, onClick, isLatest }: OrderCardProps) => {
    const { formatDateTime } = useDay({ defaultFormat: DEFAULT_DATE_FORMAT })
    return (
        <CardStyled
            isPressable
            onPress={() => onClick(order)}
            className={`w-full max-w-87.5 interactive-scale ${isLatest ? 'border-2 border-[#FFB74D]' : ''}`}
        >
            <CardHeader className="flex justify-between items-center gap-4 relative">
                {isLatest && (
                    <div className="absolute -top-3 -right-3 bg-gradient-to-r from-red-500 to-pink-500 text-white text-[10px] font-bold px-2 py-0.5 rounded-bl-lg rounded-tr-lg shadow-sm z-10 flex items-center gap-1">
                        <span>NEW</span>
                        <div className="animate-pulse bg-white rounded-full w-1.5 h-1.5" />
                    </div>
                )}

                <div className="flex flex-col items-start">
                    <p className="text-small text-default-500 uppercase font-bold flex items-center gap-2">
                        {ORDER_TEXT.labels.ORDER_CODE}
                    </p>
                    <h4 className="text-start text-black font-bold text-large">{order.code}</h4>
                </div>
                <Chip color={getOrderStatusColor(order.status)} variant="flat" size="sm">
                    {getOrderStatusLabel(order.status)}
                </Chip>
            </CardHeader>

            <Divider />

            <CardBody className="gap-2">
                <div className="flex items-center gap-2 text-default-500 text-sm">
                    <Calendar size={16} />
                    <span>{formatDateTime({ date: order.createdAt })}</span>
                </div>

                {/* <div className="flex items-center gap-2 text-default-500 text-sm">
                    <Package size={16} />
                    <span>{order.items.length} {ORDER_TEXT.labels.PRODUCTS}</span>
                </div> 
                */}
            </CardBody>

            <Divider />

            <CardFooter className="justify-between">
                <span className="text-default-500 text-sm">{ORDER_TEXT.labels.TOTAL_AMOUNT}</span>
                <span className="text-lg font-bold text-primary">
                    {formatCurrencyWithSymbol(order.totalAmount)}
                </span>
            </CardFooter>
        </CardStyled>
    )
}
