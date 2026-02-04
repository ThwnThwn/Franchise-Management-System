"use client"

import React from "react"

import { PencilIcon, TrashIcon } from "lucide-react"
import { TableStyled, ButtonIconStyled, TitleSkeleton } from "@/components"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import {
    User as UserInfo,
    Spinner,
    TableCell,
    TableRow,
    TableBody,
    Chip,
    Tooltip,
    TableHeader,
    TableColumn
} from "@heroui/react"
import { Product, ProductSize } from "../../models/products/respone"

type ProductTableProps = {
    products: Product[]
    isLoading?: boolean
    emptyMessage?: string
    onEdit?: (product: Product) => void
    onDelete?: (product: Product) => void
    isRefetching?: boolean
}

export function ProductTable({
    products,
    isLoading = false,
    emptyMessage,
    onEdit,
    onDelete,
    isRefetching = false
}: ProductTableProps) {

    // --- HELPERS ---
    // 1. Tính khoảng giá: Tìm giá thấp nhất và cao nhất trong danh sách sizes
    const getPriceDisplay = (sizes: ProductSize[]) => {
        if (!sizes || sizes.length === 0)
            return <span className="text-slate-400 italic">Chưa thiết lập giá</span>

        const prices = sizes.map((s) => s.basePrice)
        const min = Math.min(...prices)
        const max = Math.max(...prices)

        if (min === max)
            return <span className="font-semibold">{formatCurrencyWithSymbol(min)}</span>

        return (
            <div className="flex flex-col">
                <span className="text-xs text-slate-500">Từ</span>
                <span className="font-semibold text-slate-900">
                    {formatCurrencyWithSymbol(min)}
                </span>
                {/* <span className="text-xs text-slate-500">đến {formatCurrency(max)}</span> */}
            </div>
        )
    }

    // 2. Parse Attributes JSON để hiển thị tên đẹp hơn
    // Ví dụ: attributes = '{"color": "Đỏ", "material": "Cotton"}' -> "Đỏ, Cotton"
    const getSizeName = (size: ProductSize) => {
        if (size.name) return size.name // Ưu tiên tên nếu có

        if (size.attributes) {
            try {
                const attrs = JSON.parse(size.attributes)
                // Lấy tất cả value nối lại
                return Object.values(attrs).join(", ")
            } catch (e) {
                console.error(e)
                return size.sku // Fallback về SKU nếu lỗi JSON
            }
        }
        return size.sku
    }

    // 3. Render nội dung Tooltip cho cột Sizes
    const renderSizeTooltip = (sizes: ProductSize[]) => {
        return (
            <div className="px-1 py-2">
                <div className="text-xs font-bold mb-2 text-slate-300">Danh sách phân loại:</div>
                <div className="flex flex-col gap-1">
                    {sizes.map((s) => (
                        <div key={s.id} className="flex justify-between gap-4 text-xs">
                            <span className="text-white font-medium">{getSizeName(s)}</span>
                            <span className="text-emerald-400">
                                {formatCurrencyWithSymbol(s.basePrice)}
                            </span>
                        </div>
                    ))}
                </div>
            </div>
        )
    }

    // --- RENDER TABLE ---

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

        if (products.length === 0) {
            return (
                <TableRow>
                    <TableCell colSpan={6} className="text-center py-10 text-slate-500">
                        {emptyMessage ?? "Chưa có sản phẩm nào trong hệ thống"}
                    </TableCell>
                </TableRow>
            )
        }

        return products.map((item) => (
            <TableRow
                key={item.id}
                className="border-b border-gray-100 last:border-0 hover:bg-gray-50 transition-colors"
            >
                {/* Cột 1: Thông tin sản phẩm */}
                <TableCell>
                    <UserInfo
                        name={
                            <div
                                className="font-semibold text-slate-900 line-clamp-1 max-w-50"
                                title={item.name}
                            >
                                {item.name}
                            </div>
                        }
                        description={
                            <div className="line-clamp-1 max-w-50 text-xs text-slate-500">
                                {item.description || "Không có mô tả"}
                            </div>
                        }
                        avatarProps={{
                            src: item.imageUrl,
                            radius: "md",
                            size: "lg",
                            isBordered: true,
                            className: "bg-gray-100",
                            fallback: (
                                <span className="text-lg">
                                    {item.name.charAt(0)?.toUpperCase()}
                                </span>
                            )
                        }}
                    />
                </TableCell>

                {/* Cột 2: Danh mục */}
                <TableCell className="text-sm text-slate-700 font-medium">
                    {item.categoryName}
                </TableCell>

                {/* Cột 3: Giá bán (Khoảng giá) */}
                <TableCell>{getPriceDisplay(item.sizes)}</TableCell>

                {/* Cột 4: Sizes / Phân loại */}
                <TableCell>
                    <Tooltip
                        content={renderSizeTooltip(item.sizes)}
                        placement="right"
                        classNames={{
                            content: "bg-slate-900 text-white rounded-lg shadow-xl"
                        }}
                    >
                        <Chip
                            size="sm"
                            variant="solid"
                            color="secondary"
                            className="cursor-pointer"
                        >
                            {item.sizes.length} phân loại
                        </Chip>
                    </Tooltip>
                </TableCell>

                {/* Cột 5: Trạng thái */}
                <TableCell>
                    <Chip
                        size="sm"
                        variant="dot"
                        color={item.isActive ? "success" : "danger"}
                        classNames={{
                            base: "border-none bg-transparent gap-2",
                            dot: item.isActive ? "animate-pulse" : ""
                        }}
                    >
                        {item.isActive ? "Đang kinh doanh" : "Ngưng kinh doanh"}
                    </Chip>
                </TableCell>

                {/* Cột 6: Thao tác */}
                <TableCell>
                    <div className="flex items-center gap-1 justify-center">
                        <Tooltip content="Chỉnh sửa chi tiết">
                            <ButtonIconStyled
                                variant="light"
                                color="primary"
                                className="opacity-70 hover:opacity-100"
                                onPress={() => onEdit?.(item)}
                            >
                                <PencilIcon size={18} />
                            </ButtonIconStyled>
                        </Tooltip>

                        <Tooltip content="Xóa sản phẩm" color="danger">
                            <ButtonIconStyled
                                variant="light"
                                color="danger"
                                className="opacity-70 hover:opacity-100"
                                onPress={() => onDelete?.(item)}
                            >
                                <TrashIcon size={18} />
                            </ButtonIconStyled>
                        </Tooltip>
                    </div>
                </TableCell>
            </TableRow>
        ))
    }

    return (
        <>
            <TableStyled className="min-w-full" removeWrapper aria-label="Danh sách sản phẩm">
                <TableHeader>
                    <TableColumn width="30%">SẢN PHẨM</TableColumn>
                    <TableColumn width="15%">DANH MỤC</TableColumn>
                    <TableColumn width="15%">GIÁ BÁN</TableColumn>
                    <TableColumn width="15%">PHÂN LOẠI</TableColumn>
                    <TableColumn width="15%">TRẠNG THÁI</TableColumn>
                    <TableColumn width="10%" align="center">
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
