"use client"

import React, { useState } from "react"
import { Package, Search } from "lucide-react"
import { InputStyled } from "@/components"
import { InventoryTable } from "../components/InventoryTable"
import { AdjustStockModal } from "../components/AdjustStockModal"
import { useGetAllStocks, useAdjustStock } from "../hooks/useInventory"
import { StockItem } from "../models/response"
import { GetAllStockParams } from "../models/request"
import { DashboardHeader } from "@/features/dashboard/components/general/DashboardHeader"

export default function InventoryManagementPage() {
    // State
    const [filter, setFilter] = useState<GetAllStockParams>({
        page: 1,
        pageSize: 20
    })

    // Fetch stocks
    const {
        data: stocksData,
        isLoading,
        isRefetching
    } = useGetAllStocks({
        params: filter
    })
    const stocks = stocksData?.stocks || []

    // Adjust stock modal
    const [selectedStock, setSelectedStock] = useState<StockItem | null>(null)
    const [adjustType, setAdjustType] = useState<"increase" | "decrease">("increase")
    const [isModalOpen, setIsModalOpen] = useState(false)

    const adjustMutation = useAdjustStock({
        onSuccess: () => {
            setIsModalOpen(false)
            setSelectedStock(null)
        }
    })

    // Handlers
    const handleIncrease = (stock: StockItem) => {
        setSelectedStock(stock)
        setAdjustType("increase")
        setIsModalOpen(true)
    }

    const handleDecrease = (stock: StockItem) => {
        setSelectedStock(stock)
        setAdjustType("decrease")
        setIsModalOpen(true)
    }

    const handleAdjustSubmit = (quantity: number) => {
        if (!selectedStock) return

        adjustMutation.mutate({
            storeId: selectedStock.storeId,
            items: [
                {
                    ingredientId: selectedStock.ingredient.id,
                    quantity: adjustType === "decrease" ? -quantity : quantity
                }
            ]
        })
    }

    return (
        <div className="max-w-7xl mb-8 mx-auto min-h-screen p-4 md:p-6 lg:p-8 bg-slate-50/30">
            <DashboardHeader
                icon={<Package className="w-6 h-6 text-white" />}
                title="Quản lý kho"
                description="Theo dõi và điều chỉnh tồn kho nguyên liệu"
            />

            {/* --- TOOLBAR --- */}
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6 bg-white p-4 rounded-2xl border border-slate-100 shadow-sm">
                <div className="flex flex-col sm:flex-row gap-3 w-full sm:w-auto">
                    {/* Search Input */}
                    <div className="w-full sm:w-80">
                        <InputStyled
                            placeholder="Tìm kiếm nguyên liệu..."
                            startContent={<Search size={18} className="text-slate-400" />}
                            size="md"
                            onChange={(e) => {
                                // TODO: Add search functionality
                                console.log("Search:", e.target.value)
                            }}
                        />
                    </div>
                </div>
            </div>

            {/* --- STOCK COUNT --- */}
            <div className="mb-4 flex items-center justify-between">
                <div className="text-sm text-slate-600">
                    Tổng số:{" "}
                    <span className="font-semibold text-slate-900">
                        {stocksData?.totalCount || 0}
                    </span>{" "}
                    mục
                </div>
            </div>

            {/* --- TABLE --- */}
            <div className="bg-white rounded-2xl shadow-sm border border-slate-100 overflow-hidden">
                <InventoryTable
                    stocks={stocks}
                    isLoading={isLoading}
                    isRefetching={isRefetching}
                    onIncrease={handleIncrease}
                    onDecrease={handleDecrease}
                />
            </div>

            {/* --- ADJUST MODAL --- */}
            <AdjustStockModal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                stock={selectedStock}
                type={adjustType}
                onSubmit={handleAdjustSubmit}
                isLoading={adjustMutation.isPending}
            />
        </div>
    )
}
