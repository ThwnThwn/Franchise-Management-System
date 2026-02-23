"use client"

import React from "react"
import { Plus, Minus } from "lucide-react"
import { TableStyled, ButtonIconStyled, TitleSkeleton } from "@/components"
import {
    TableCell,
    TableRow,
    TableBody,
    Chip,
    Tooltip,
    TableHeader,
    TableColumn,
    Spinner
} from "@heroui/react"
import { StockItem } from "../../models/response"
import { StoreDetailDto } from "@/features/stores/models/schema/response"

type InventoryTableProps = {
    stocks: StockItem[]
    stores?: StoreDetailDto[]
    isLoading?: boolean
    emptyMessage?: string
    onIncrease?: (stock: StockItem) => void
    onDecrease?: (stock: StockItem) => void
    isRefetching?: boolean
}

export function InventoryTable({
    stocks,
    stores = [],
    isLoading = false,
    emptyMessage,
    onIncrease,
    onDecrease,
    isRefetching = false
}: InventoryTableProps) {
    // Helper: Get stock status color and label
    const getStockStatus = (stockStatus: number) => {
        switch (stockStatus) {
            case 0:
                return { color: "danger" as const, label: "Sắp hết" }
            case 1:
                return { color: "success" as const, label: "Bình thường" }
            case 2:
                return { color: "primary" as const, label: "Dư thừa" }
            default:
                return { color: "default" as const, label: "Không xác định" }
        }
    }

    // Helper: format quantity string từ BE (có thể là "10", "10.5", "0", "")
    const formatQty = (raw: string): string => {
        const n = parseFloat(raw)
        if (isNaN(n)) return "—"
        // Bỏ trailing zeros: 10.50 → "10.5", 10.00 → "10"
        return n % 1 === 0 ? n.toString() : n.toString()
    }

    // Render table body
    const renderBody = () => {
        if (isLoading) {
            return Array.from({ length: 5 }).map((_, index) => (
                <TableRow key={`skeleton-${index}`} className="h-20">
                    <TableCell colSpan={6} className="text-center">
                        <TitleSkeleton className="h-12 w-full" />
                    </TableCell>
                </TableRow>
            ))
        }

        if (stocks.length === 0) {
            return (
                <TableRow>
                    <TableCell colSpan={6} className="text-center py-10 text-slate-500">
                        {emptyMessage ?? "Không có dữ liệu tồn kho"}
                    </TableCell>
                </TableRow>
            )
        }

        return stocks.map((item) => {
            const status = getStockStatus(item.stockStatus)
            return (
                <TableRow
                    key={`${item.storeId}-${item.ingredientViewRes.id}`}
                    className="border-b border-gray-100 last:border-0 hover:bg-gray-50 transition-colors"
                >
                    {/* Cột 1: Nguyên liệu */}
                    <TableCell>
                        <div>
                            <div className="font-semibold text-slate-900">
                                {item.ingredientViewRes.name}
                            </div>
                            <div className="text-xs text-slate-500">
                                {item.ingredientViewRes.categoryId}
                            </div>
                        </div>
                    </TableCell>

                    {/* Cột 2: Cửa hàng */}
                    <TableCell className="text-sm text-slate-700">
                        {(() => {
                            const store = stores.find((s) => s.id === item.storeId)
                            return store ? (
                                <div className="flex flex-col">
                                    <span className="font-medium">{store.name}</span>
                                    <span className="text-xs text-slate-400">{store.address}</span>
                                </div>
                            ) : (
                                <span className="text-xs text-slate-400 font-mono">{item.storeId}</span>
                            )
                        })()}
                    </TableCell>

                    {/* Cột 3: Số lượng tồn */}
                    <TableCell>
                        <div className="flex flex-col">
                            <span className="font-semibold text-slate-900">
                                {formatQty(item.quantity)}
                            </span>
                            <span className="text-xs text-slate-500">
                                {item.ingredientViewRes.baseUnit}
                            </span>
                        </div>
                    </TableCell>

                    {/* Cột 4: Ngưỡng tối thiểu */}
                    <TableCell className="text-sm text-slate-600">
                        {formatQty(item.minThreshold)} {item.ingredientViewRes.baseUnit}
                    </TableCell>

                    {/* Cột 5: Trạng thái */}
                    <TableCell>
                        <Chip
                            size="sm"
                            variant="dot"
                            color={status.color}
                            classNames={{
                                base: "border-none bg-transparent gap-2",
                                dot: item.stockStatus === 0 ? "animate-pulse" : ""
                            }}
                        >
                            {status.label}
                        </Chip>
                    </TableCell>

                    {/* Cột 6: Thao tác */}
                    <TableCell>
                        <div className="flex items-center gap-1 justify-center">
                            <Tooltip content="Nhập kho">
                                <ButtonIconStyled
                                    variant="light"
                                    color="success"
                                    className="opacity-70 hover:opacity-100"
                                    onPress={() => onIncrease?.(item)}
                                >
                                    <Plus size={18} />
                                </ButtonIconStyled>
                            </Tooltip>

                            <Tooltip content="Xuất kho" color="warning">
                                <ButtonIconStyled
                                    variant="light"
                                    color="warning"
                                    className="opacity-70 hover:opacity-100"
                                    onPress={() => onDecrease?.(item)}
                                >
                                    <Minus size={18} />
                                </ButtonIconStyled>
                            </Tooltip>
                        </div>
                    </TableCell>
                </TableRow>
            )
        })
    }

    return (
        <>
            <TableStyled className="min-w-full" removeWrapper aria-label="Danh sách tồn kho">
                <TableHeader>
                    <TableColumn width="25%">NGUYÊN LIỆU</TableColumn>
                    <TableColumn width="15%">CỬA HÀNG</TableColumn>
                    <TableColumn width="15%">SỐ LƯỢNG TỒN</TableColumn>
                    <TableColumn width="15%">NGƯỠNG TỐI THIỂU</TableColumn>
                    <TableColumn width="15%">TRẠNG THÁI</TableColumn>
                    <TableColumn width="15%" align="center">
                        THAO TÁC
                    </TableColumn>
                </TableHeader>
                <TableBody>{renderBody()}</TableBody>
            </TableStyled>

            {/* Loading Indicator nhỏ khi refetch */}
            {isRefetching && (
                <div className="w-full text-center py-2 text-xs text-slate-400 border-t border-slate-50 bg-slate-50/50">
                    <Spinner />
                </div>
            )}
        </>
    )
}
