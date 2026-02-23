"use client"

import { TableHeader, TableColumn, TableBody, TableRow, TableCell, Chip } from "@heroui/react"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import { OrderStatusColors, OrderStatusLabels } from "@/features/orders/constants"
import { useDay } from "@/hooks"
import { useRouter } from "next/navigation"
import { OrderSummary } from "@/features/orders/models/response"
import { TableStyled } from "@/components"

interface OrdersTableProps {
    orders: OrderSummary[]
    isLoading: boolean
}

export const OrdersTable = ({ orders, isLoading }: OrdersTableProps) => {
    const router = useRouter()
    const { formatDateTime } = useDay()

    return (
        <TableStyled
            aria-label="Danh sách đơn hàng"
            selectionMode="single"
            onRowAction={(key) => router.push(`/dashboard/orders/${key}`)}
            className="cursor-pointer"
            classNames={{
                wrapper: "shadow-none border border-divider p-0 rounded-2xl overflow-hidden",
                th: "bg-default-100 text-default-600 font-bold",
                tr: "hover:bg-default-50 transition-colors"
            }}
        >
            <TableHeader>
                <TableColumn className="w-[30%]">MÃ ĐƠN</TableColumn>
                <TableColumn className="w-[20%]">THỜI GIAN</TableColumn>
                <TableColumn className="w-[30%] text-center">TỔNG TIỀN</TableColumn>
                <TableColumn className="w-[20%] text-center">TRẠNG THÁI</TableColumn>
            </TableHeader>
            <TableBody
                items={orders}
                isLoading={isLoading}
                loadingContent={<div className="p-10 text-primary">Đang tải dữ liệu...</div>}
                emptyContent={"Không tìm thấy đơn hàng nào."}
            >
                {(item) => (
                    <TableRow key={item.id}>
                        <TableCell className="font-bold text-slate-800">#{item.code}</TableCell>
                        <TableCell className="text-xs text-default-500">
                            {formatDateTime({ date: item.createdAt, format: "DD/MM/YYYY HH:mm" })}
                        </TableCell>
                        <TableCell className="text-center font-black text-primary">
                            {formatCurrencyWithSymbol(item.totalAmount)}
                        </TableCell>
                        <TableCell className="text-center">
                            <Chip
                                size="sm"
                                variant="flat"
                                color={OrderStatusColors[item.status]}
                                className="font-black border-none"
                            >
                                {OrderStatusLabels[item.status]}
                            </Chip>
                        </TableCell>
                    </TableRow>
                )}
            </TableBody>
        </TableStyled>
    )
}
