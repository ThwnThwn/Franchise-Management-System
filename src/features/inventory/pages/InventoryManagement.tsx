"use client"

import { useMemo, useState } from "react"
import { Package, Search } from "lucide-react"
import { InputStyled, PaginationStyled } from "@/components"
import { InventoryTable } from "../components/InventoryTable"
import { AdjustStockModal } from "../components/AdjustStockModal"
import { useGetStockByStoreId, useAdjustStock } from "../hooks/useInventory"
import { StockItem } from "../models/response"
import { GetAllStockParams } from "../models/request"
import { DashboardHeader } from "@/features/dashboard/components/general/DashboardHeader"
import { useTokenStore } from "@/hooks"
import { debouncedWrapper } from "@/utils/helpers/axiosHelper"

/**
 * @title Trang cho Manager quản lí kho của cửa hàng
 */
export default function InventoryManagementPage() {
    const storeId = useTokenStore((s) => s.storeId)

    // ── Store filter ─────────────────────────────────────────
    // const [selectedStoreId, setSelectedStoreId] = useState<string>(storeId || "")

    // const { data: storesData } = useGetStores({
    //     pagination: { page: 1, pageSize: 100 },
    //     params: {}
    // })
    // const stores = storesData?.items ?? []

    // ── Stock filter ─────────────────────────────────────────
    const [filter, setFilter] = useState<Omit<GetAllStockParams, "storeIds">>({
        // storeIds: storeId ? [storeId] : [],
        page: 1,
        pageSize: 50
    })

    // Reset to page 1 when store changes
    // const handleStoreChange = (storeId: string) => {
    //     setSelectedStoreId(storeId)
    //     setFilter((prev) => ({ ...prev, page: 1 }))
    // }

    const baseParams: Omit<GetAllStockParams, "storeIds"> = { ...filter }

    // Use per-store endpoint when store selected (storeIds is commented out in BE)
    // const allStocksQuery = useGetAllStocks({
    //     params: baseParams,
    //     enabled: !selectedStoreId
    // })

    const storeStocksQuery = useGetStockByStoreId({
        storeId: storeId || "",
        params: baseParams,
        enabled: !!storeId
    })

    const activeQuery = storeStocksQuery
    const stocksData = activeQuery.data
    const isLoading = activeQuery.isLoading
    const isRefetching = activeQuery.isRefetching
    const stocks = stocksData?.items || []

    // ── Adjust stock modal ───────────────────────────────────
    const [selectedStock, setSelectedStock] = useState<StockItem | null>(null)
    const [adjustType, setAdjustType] = useState<"increase" | "decrease">("increase")
    const [isModalOpen, setIsModalOpen] = useState(false)

    const adjustMutation = useAdjustStock({
        onSuccess: () => {
            setIsModalOpen(false)
            setSelectedStock(null)
        }
    })

    // ── Handlers ─────────────────────────────────────────────
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
            storeId: storeId || selectedStock.storeId,
            items: [
                {
                    ingredientId: selectedStock.ingredientViewRes.id,
                    quantity: adjustType === "decrease" ? -quantity : quantity
                }
            ]
        })
    }

    const [isTyping, setIsTyping] = useState(false)
    const debouncedSearch = useMemo(
        () =>
            debouncedWrapper(
                async (value: string) => {
                    setFilter((prev) => ({ ...prev, keyword: value || undefined, page: 1 }))
                },
                1000,
                () => setIsTyping(true),
                () => setIsTyping(false)
            ),
        []
    )

    // const handleSearch = (value: string) => {
    //     // TODO: BE filter by keyword khi có
    //     setFilter((prev) => ({ ...prev, keyword: value || undefined, page: 1 }))
    // }

    // Selected store label
    // const selectedStore = stores.find((s) => s.id === selectedStoreId)

    return (
        <div className="max-w-7xl mb-8 mx-auto min-h-screen p-4 md:p-6 lg:p-8 bg-slate-50/30">
            <DashboardHeader
                icon={<Package className="w-6 h-6 text-white" />}
                title="Quản lý kho"
                description="Theo dõi và điều chỉnh tồn kho nguyên liệu"
            />

            {/* ── TOOLBAR ── */}
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6 bg-white p-4 rounded-2xl border border-slate-100 shadow-sm">
                <div className="flex flex-col sm:flex-row gap-3 w-full">
                    {/* Search */}
                    <div className="w-full sm:w-72">
                        <InputStyled
                            placeholder="Tìm kiếm nguyên liệu..."
                            startContent={<Search size={18} className="text-slate-400" />}
                            size="md"
                            onChange={async (e) => {
                                await debouncedSearch(e.target.value)
                            }}
                        />
                    </div>

                    {/* Store filter */}
                    {/* <div className="w-full sm:w-72" hidden={!!storeId}>
                        <Select
                            placeholder="Tất cả cửa hàng"
                            selectedKeys={selectedStoreId ? [selectedStoreId] : []}
                            onChange={(e) => handleStoreChange(e.target.value)}
                            variant="bordered"
                            startContent={<Store className="w-4 h-4 text-slate-400" />}
                            classNames={{
                                trigger: "border-slate-200 hover:border-primary-400 h-10 bg-white"
                            }}
                        >
                            {stores.map((store) => (
                                <SelectItem key={store.id} textValue={store.name}>
                                    <div className="flex flex-col">
                                        <span className="font-medium">{store.name}</span>
                                        <span className="text-xs text-slate-400">
                                            {store.address}
                                        </span>
                                    </div>
                                </SelectItem>
                            ))}
                        </Select>
                    </div> */}

                    {/* Clear store filter badge */}
                    {/* {selectedStoreId && selectedStore && (
                        <div className="flex items-center">
                            <Chip
                                color="primary"
                                variant="flat"
                                size="sm"
                                isCloseable={!storeId}
                                onClose={!storeId ? () => handleStoreChange("") : undefined}
                                classNames={{ base: "h-9 px-3" }}
                            >
                                {selectedStore.name}
                            </Chip>
                        </div>
                    )} */}
                </div>
            </div>

            {/* ── COUNT ── */}
            <div className="mb-4 flex items-center justify-between">
                <div className="text-sm text-slate-600">
                    {/* {selectedStore ? (
                        <>
                            Kho của{" "}
                            <span className="font-semibold text-primary-600">
                                {selectedStore.name}
                            </span>
                            {" — "}
                        </>
                    ) : (
                        "Tất cả cửa hàng — "
                    )} */}
                    <span className="font-semibold text-slate-900">
                        {stocksData?.totalCount || 0}
                    </span>{" "}
                    mục
                </div>
            </div>

            {/* ── TABLE ── */}
            <div className="bg-white rounded-2xl shadow-sm border border-slate-100 overflow-hidden">
                <InventoryTable
                    stocks={stocks}
                    // stores={stores}
                    isLoading={isLoading}
                    isRefetching={isRefetching}
                    onIncrease={handleIncrease}
                    onDecrease={handleDecrease}
                />

                {/* Pagination */}
                {(stocksData?.totalPages ?? 0) > 1 && !isTyping && (
                    <PaginationStyled
                        page={filter.page ?? 1}
                        total={stocksData?.totalPages ?? 1}
                        onChange={(p) => setFilter((prev) => ({ ...prev, page: p }))}
                        // showControls
                        // color="primary"
                        // variant="light"
                    />
                )}
            </div>

            {/* ── ADJUST MODAL ── */}
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
