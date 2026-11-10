"use client"

import React from "react"
import { Package, Pencil, Plus } from "lucide-react"
import { TableStyled, ButtonIconStyled, TitleSkeleton, ButtonStyled } from "@/components"
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
import { IngredientItem, IngredientCategory } from "../../models/response"

type IngredientsTableProps = {
    ingredients: IngredientItem[]
    categories: IngredientCategory[]
    isLoading?: boolean
    isRefetching?: boolean
    emptyMessage?: string
    onAddStock?: (ingredient: IngredientItem) => void
    onEdit?: (ingredient: IngredientItem) => void
    // onDelete?: (ingredient: IngredientItem) => void
}

export function IngredientsTable({
    ingredients,
    categories,
    isLoading = false,
    isRefetching = false,
    emptyMessage,
    onAddStock,
    onEdit
    // onDelete
}: IngredientsTableProps) {
    /** Lấy tên danh mục từ categoryId */
    const getCategoryName = (categoryId: string | null): string => {
        if (!categoryId) return "—"
        const cat = categories.find((c) => c.id === categoryId)
        return cat?.name ?? "—"
    }

    /** Mapping đơn vị → màu chip */
    const getUnitColor = (unit: string) => {
        const liquidUnits = ["l", "ml", "liters", "liter", "mililit"]
        const isLiquid = liquidUnits.some((u) => unit.toLowerCase().includes(u))
        return isLiquid ? ("primary" as const) : ("success" as const)
    }

    // ─── Render body ────────────────────────────────────────────────
    const renderBody = () => {
        if (isLoading) {
            return Array.from({ length: 5 }).map((_, index) => (
                <TableRow key={`skeleton-${index}`} className="h-14">
                    <TableCell colSpan={4} className="text-center">
                        <TitleSkeleton className="h-10 w-full" />
                    </TableCell>
                </TableRow>
            ))
        }

        if (ingredients.length === 0) {
            return (
                <TableRow>
                    <TableCell colSpan={4} className="text-center py-14 text-slate-400">
                        <div className="flex flex-col items-center gap-2">
                            <Package className="w-10 h-10 text-slate-300" />
                            <span className="text-sm">
                                {emptyMessage ?? "Chưa có nguyên liệu nào"}
                            </span>
                        </div>
                    </TableCell>
                </TableRow>
            )
        }

        return ingredients.map((item) => (
            <TableRow
                key={item.id}
                className="border-b border-gray-100 last:border-0 hover:bg-slate-50 transition-colors"
            >
                {/* Tên nguyên liệu */}
                <TableCell>
                    <div className="flex items-center gap-3">
                        <div className="w-9 h-9 rounded-xl bg-linear-to-br from-emerald-500 to-teal-600 flex items-center justify-center shrink-0">
                            <Package className="w-4 h-4 text-white" />
                        </div>
                        <div>
                            <div className="font-semibold text-slate-900 text-sm">{item.name}</div>
                            {/* <div className="text-xs text-slate-400 font-mono">
                                {item.id.slice(0, 8)}…
                            </div> */}
                        </div>
                    </div>
                </TableCell>

                {/* Danh mục */}
                <TableCell>
                    <span className="text-sm text-slate-700">
                        {getCategoryName(item.categoryId)}
                    </span>
                </TableCell>

                {/* Đơn vị */}
                <TableCell>
                    <Chip
                        size="sm"
                        variant="flat"
                        color={getUnitColor(item.baseUnit)}
                        classNames={{ base: "font-mono" }}
                    >
                        {item.baseUnit}
                    </Chip>
                </TableCell>

                {/* Thao tác */}
                <TableCell>
                    <div className="flex items-center gap-1 justify-center">
                        <Tooltip content="Nhập kho nguyên liệu" color="success">
                            <ButtonStyled
                                size="sm"
                                color="success"
                                variant="flat"
                                className="text-xs"
                                startContent={<Plus size={14} />}
                                onPress={() => onAddStock?.(item)}
                            >
                                Nhập kho
                            </ButtonStyled>
                        </Tooltip>

                        <Tooltip content="Chỉnh sửa">
                            <ButtonIconStyled
                                variant="light"
                                color="default"
                                className="opacity-60 hover:opacity-100"
                                onPress={() => onEdit?.(item)}
                            >
                                <Pencil size={16} />
                            </ButtonIconStyled>
                        </Tooltip>

                        {/* <Tooltip content="Xóa" color="danger">
                            <ButtonIconStyled
                                variant="light"
                                color="danger"
                                className="opacity-60 hover:opacity-100"
                                onPress={() => onDelete?.(item)}
                            >
                                <Trash2 size={16} />
                            </ButtonIconStyled>
                        </Tooltip> */}
                    </div>
                </TableCell>
            </TableRow>
        ))
    }

    return (
        <>
            <TableStyled className="min-w-full" removeWrapper aria-label="Danh sách nguyên liệu">
                <TableHeader>
                    <TableColumn width="35%">NGUYÊN LIỆU</TableColumn>
                    <TableColumn width="25%">DANH MỤC</TableColumn>
                    <TableColumn width="15%">ĐƠN VỊ</TableColumn>
                    <TableColumn width="25%" align="center">
                        THAO TÁC
                    </TableColumn>
                </TableHeader>
                <TableBody>{renderBody()}</TableBody>
            </TableStyled>

            {isRefetching && (
                <div className="w-full text-center py-2 border-t border-slate-50 bg-slate-50/50">
                    <Spinner size="sm" />
                </div>
            )}
        </>
    )
}
