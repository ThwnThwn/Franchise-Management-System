"use client"

import React, { useState } from "react"
import { Package, Search } from "lucide-react"
import { InputStyled } from "@/components"
import { InventoryTable } from "../components/InventoryTable"
import { AdjustStockModal } from "../components/AdjustStockModal"
import { useGetAllStocks, useAdjustStock } from "../hooks/useInventory"
import { StockItem } from "../models/response"
import { GetAllStockParams } from "../models/request"

export default function InventoryManagementPage() {
    // State
    const [filter, setFilter] = useState<GetAllStockParams>({
        page: 1,
        pageSize: 20
    })

    // Fetch stocks
    const { data: stocksData, isLoading, isRefetching } = useGetAllStocks({
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
        <div className="p-6">
            {/* --- HEADER --- */}
            <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-3">
                    <div className="p-2 bg-gradient-to-br from-purple-500 to-purple-600 rounded-xl shadow-lg">
                        <Package className="text-white" size={24} />
                    </div>
                    <div>
                        <h1 className="text-2xl font-bold text-slate-900">Quản lý kho</h1>
                        <p className="text-sm text-slate-500">Theo dõi và điều chỉnh tồn kho nguyên liệu</p>
                    </div>
                </div>
            </div>

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
                    Tổng số: <span className="font-semibold text-slate-900">{stocksData?.totalCount || 0}</span> mục
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
