"use client"

import { useState } from "react"
import { FlaskConical, Search, Plus } from "lucide-react"
import { InputStyled, ButtonStyled } from "@/components"
import { DashboardHeader } from "@/features/dashboard/components/general/DashboardHeader"
import { IngredientsTable } from "../components/IngredientsTable"
import { CreateIngredientModal } from "../components/CreateIngredientModal"
import { EditIngredientModal } from "../components/EditIngredientModal"
import {
    useGetIngredients,
    useGetIngredientCategories,
    useCreateIngredient,
    useUpdateIngredient,
    useAddIngredientStock
} from "../hooks/useInventory"
import { IngredientItem } from "../models/response"
import { GetIngredientsParams } from "../models/request"
import { useGetStores } from "@/features/stores"
import { AddIngredientStockModal } from "../components/AddIngredientStockModal"

/**
 * @title Trang cho Admin quản lí danh sách định nghĩa nguyên liệu của hệ thống
 */
export default function IngredientsPage() {
    // ── Filter state ──────────────────────────────────────────────
    const [filter, setFilter] = useState<GetIngredientsParams>({
        page: 1,
        pageSize: 20
    })
    const [search, setSearch] = useState("")

    // ── Data fetching ─────────────────────────────────────────────
    const { data: ingredientsData, isLoading, isRefetching } = useGetIngredients({ params: filter })

    const { data: categories = [] } = useGetIngredientCategories()

    // ── Stores (for stock modal) ───────────────────────────────
    const { data: storesData } = useGetStores({
        pagination: { page: 1, pageSize: 100 },
        params: {}
    })
    const stores = storesData?.items ?? []

    const ingredients = ingredientsData?.items ?? []
    const totalCount = ingredientsData?.totalCount ?? 0

    // ── Add Stock Modal state ─────────────────────────────────────
    const [selectedIngredient, setSelectedIngredient] = useState<IngredientItem | null>(null)
    const [isAddStockOpen, setIsAddStockOpen] = useState(false)

    const addStockMutation = useAddIngredientStock({
        onSuccess: () => {
            setIsAddStockOpen(false)
            setSelectedIngredient(null)
        }
    })

    // ── Create Ingredient Modal state ────────────────────────────
    const [isCreateOpen, setIsCreateOpen] = useState(false)

    const createIngredientMutation = useCreateIngredient({
        onSuccess: () => setIsCreateOpen(false)
    })

    // ── Edit Ingredient Modal state ──────────────────────────
    const [editingIngredient, setEditingIngredient] = useState<IngredientItem | null>(null)
    const [isEditOpen, setIsEditOpen] = useState(false)

    const updateIngredientMutation = useUpdateIngredient({
        onSuccess: () => {
            setIsEditOpen(false)
            setEditingIngredient(null)
        }
    })

    // ── Handlers ──────────────────────────────────────────────────

    /** Mở modal nhập kho cho nguyên liệu cụ thể */
    const handleAddStock = (ingredient: IngredientItem) => {
        setSelectedIngredient(ingredient)
        setIsAddStockOpen(true)
    }

    /** Nhập kho: gọi PUT /stores/:storeId/stock — storeId đến từ modal */
    const handleAddStockSubmit = (params: {
        storeId: string
        ingredientId: string
        quantity: number
        minThreshold?: number
        unit: string
        supplierInfo: string
        expirationDate: string
        reason: string
    }) => {
        addStockMutation.mutate({
            storeId: params.storeId,
            items: [
                {
                    ingredientId: params.ingredientId,
                    quantity: params.quantity,
                    minThreshold: params.minThreshold
                }
            ]
        })
    }

    /** Xóa nguyên liệu */
    // const deleteIngredientMutation = useDeleteIngredient()
    // const handleDelete = (ingredient: IngredientItem) => {
    //     if (
    //         !confirmDialog({
    //             title: "Xoá nguyên liệu",
    //             message: `Bạn có chắc muốn xóa "${ingredient.name}"?`
    //         })
    //     )
    //         return
    //     deleteIngredientMutation.mutate(ingredient.id)
    // }

    /** Mở modal chỉnh sửa nguyên liệu */
    const handleEdit = (ingredient: IngredientItem) => {
        setEditingIngredient(ingredient)
        setIsEditOpen(true)
    }

    /** Tìm kiếm (debounce-less cho đơn giản) */
    const handleSearch = (value: string) => {
        setSearch(value)
        // Khi BE hỗ trợ keyword query thì truyền vào filter
        setFilter((prev) => ({ ...prev, keyword: value || undefined, page: 1 }))
    }

    // ──────────────────────────────────────────────────────────────
    return (
        <div className="max-w-7xl mb-8 mx-auto min-h-screen p-4 md:p-6 lg:p-8 bg-slate-50/30">
            <DashboardHeader
                icon={<FlaskConical className="w-6 h-6 text-white" />}
                title="Quản lý nguyên liệu"
                description="Thêm, chỉnh sửa và nhập kho nguyên liệu cho hệ thống"
            />

            {/* ── TOOLBAR ── */}
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6 bg-white p-4 rounded-2xl border border-slate-100 shadow-sm">
                <div className="flex flex-col sm:flex-row gap-3 w-full sm:w-auto">
                    <div className="w-full sm:w-80">
                        <InputStyled
                            placeholder="Tìm kiếm nguyên liệu..."
                            startContent={<Search size={18} className="text-slate-400" />}
                            size="md"
                            value={search}
                            onChange={(e) => handleSearch(e.target.value)}
                        />
                    </div>
                </div>

                {/* Nút thêm nguyên liệu mới */}
                <ButtonStyled
                    color="primary"
                    startContent={<Plus size={18} />}
                    onPress={() => setIsCreateOpen(true)}
                >
                    Thêm nguyên liệu
                </ButtonStyled>
            </div>

            {/* ── COUNT ── */}
            <div className="mb-4 flex items-center justify-between">
                <div className="text-sm text-slate-600">
                    Tổng số: <span className="font-semibold text-slate-900">{totalCount}</span>{" "}
                    nguyên liệu
                </div>
            </div>

            {/* ── TABLE ── */}
            <div className="bg-white rounded-2xl shadow-sm border border-slate-100 overflow-hidden">
                <IngredientsTable
                    ingredients={ingredients}
                    categories={categories}
                    isLoading={isLoading}
                    isRefetching={isRefetching}
                    onAddStock={handleAddStock}
                    onEdit={handleEdit}
                    // onDelete={handleDelete}
                />
            </div>

            {/* ── ADD STOCK MODAL ── */}
            {selectedIngredient && (
                <AddIngredientStockModal
                    isOpen={isAddStockOpen}
                    onClose={() => {
                        setIsAddStockOpen(false)
                        setSelectedIngredient(null)
                    }}
                    initIngre={selectedIngredient}
                    ingredients={ingredients}
                    stores={stores}
                    onSubmit={handleAddStockSubmit}
                    isLoading={addStockMutation.isPending}
                />
            )}

            {/* ── CREATE INGREDIENT MODAL ── */}
            <CreateIngredientModal
                isOpen={isCreateOpen}
                onClose={() => setIsCreateOpen(false)}
                categories={categories}
                onSubmit={(req) => createIngredientMutation.mutate(req)}
                isLoading={createIngredientMutation.isPending}
            />

            {/* ── EDIT INGREDIENT MODAL ── */}
            <EditIngredientModal
                isOpen={isEditOpen}
                onClose={() => {
                    setIsEditOpen(false)
                    setEditingIngredient(null)
                }}
                ingredient={editingIngredient}
                categories={categories}
                onSubmit={(id, req) => updateIngredientMutation.mutate({ id, req })}
                isLoading={updateIngredientMutation.isPending}
            />
        </div>
    )
}
