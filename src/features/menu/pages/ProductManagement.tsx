"use client"

import React, { Suspense, useCallback, useMemo, useState } from "react"
import { Coffee, Filter, Plus, Search, X } from "lucide-react"
import {
    Dropdown,
    DropdownTrigger,
    DropdownMenu,
    DropdownItem,
    useDisclosure,
    Chip
} from "@heroui/react"
import { useDeleteProduct, useGetAllProducts } from "../hooks"
import { useGetAllCategories } from "../hooks/useCategory"
import { Product } from "../models/products/respone"
import { AlertModal, ButtonIconStyled, InputStyled, SpinnerStyled } from "@/components"
import { ProductTable } from "../components/ProductTable"
import { ProductFormModal } from "../components"
import { GetAllProductParams } from "../models/products/request"
import { debouncedWrapper } from "@/utils/helpers/axiosHelper"

// Import Components & Hooks

export default function ProductManagementPage() {
    // Search & Filter State
    const [filter, setFilter] = useState<GetAllProductParams>({})

    const handleSetFilter = useCallback((newFilter: GetAllProductParams) => {
        setFilter(newFilter)
    }, [])

    const [isTyping, setIsTyping] = useState(false)
    const debouncedSearch = useMemo(
        () =>
            debouncedWrapper(
                async (params: GetAllProductParams) => {
                    handleSetFilter(params)
                },
                500,
                () => setIsTyping(true),
                () => setIsTyping(false)
            ),
        [handleSetFilter]
    )

    // Fetch Data with filters
    const {
        data: productsData,
        isLoading,
        isRefetching
    } = useGetAllProducts({
        query: filter
    })
    const products = productsData?.products || []

    // Fetch categories for filter
    const { data: categories = [] } = useGetAllCategories()

    // State & Hooks quản lý Modal
    const [selectedProduct, setSelectedProduct] = useState<Product | null>(null)

    // Chỉ cần 1 biến state cho modal
    const { isOpen, onOpen, onOpenChange } = useDisclosure()

    // Handler: Mở để Tạo mới
    const handleCreate = () => {
        setSelectedProduct(null) // Quan trọng: set null để modal biết là Create
        onOpen()
    }

    // Handler: Mở để Update
    const handleEdit = (product: Product) => {
        setSelectedProduct(product) // Truyền product vào để modal biết là Edit
        onOpen()
    }

    // Modal Delete
    const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false)
    const deleteMutation = useDeleteProduct()

    // Mở Modal Xóa
    const handleDeleteClick = (product: Product) => {
        setSelectedProduct(product)
        setIsDeleteModalOpen(true)
    }

    // Xác nhận Xóa
    const handleConfirmDelete = async () => {
        if (!selectedProduct) return
        try {
            await deleteMutation.mutateAsync({ id: selectedProduct.id })
            setIsDeleteModalOpen(false)
            setSelectedProduct(null)
        } catch (error) {
            console.error("Delete failed:", error)
        }
    }

    // Get selected category name for display
    const selectedCategory = categories.find((cat) => cat.id === filter.categoryId)

    // Clear filters
    const handleClearFilters = () => {
        setFilter({})
    }

    const hasActiveFilters = Boolean(filter.keyword || filter.categoryId)

    return (
        <div className="max-w-7xl mx-auto min-h-screen p-4 md:p-6 lg:p-8 bg-slate-50/30">
            {/* --- HEADER --- */}
            <div className="mb-8">
                <div className="flex items-center gap-3 mb-2">
                    <div className="w-12 h-12 rounded-xl bg-linear-to-br from-primary to-secondary flex items-center justify-center">
                        <Coffee className="w-6 h-6 text-white" />
                    </div>
                    <div>
                        <h1 className="text-2xl font-black text-slate-800">Quản lý sản phẩm</h1>
                        <p className="text-slate-500 text-sm">
                            Quản lý danh sách, giá cả và danh mục
                        </p>
                    </div>
                </div>

                <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6 bg-white p-4 rounded-2xl border border-slate-100 shadow-sm">
                    <div className="flex flex-col sm:flex-row gap-3 w-full sm:w-auto">
                        <div className="w-full sm:w-80">
                            <InputStyled
                                placeholder="Tìm kiếm theo tên sản phẩm..."
                                startContent={<Search size={18} className="text-slate-400" />}
                                variant="bordered"
                                className="w-full"
                                size="md"
                                onChange={async (e) => {
                                    await debouncedSearch({
                                        ...filter,
                                        keyword: e.target.value.trim()
                                    })
                                }}
                            />
                        </div>

                        {/* Category Filter Dropdown */}
                        <Dropdown>
                            <DropdownTrigger>
                                <ButtonIconStyled
                                    variant="bordered"
                                    className={`min-w-35 ${filter.categoryId ? "border-primary text-primary" : ""}`}
                                >
                                    <Filter size={18} />
                                    <span className="ml-2">
                                        {selectedCategory?.name || "Danh mục"}
                                    </span>
                                </ButtonIconStyled>
                            </DropdownTrigger>
                            <DropdownMenu
                                aria-label="Category Filter"
                                selectionMode="single"
                                selectedKeys={filter.categoryId ? [filter.categoryId] : []}
                                onSelectionChange={(keys) => {
                                    const selected = Array.from(keys)[0] as string
                                    handleSetFilter({
                                        ...filter,
                                        categoryId: selected || undefined
                                    })
                                }}
                            >
                                {[
                                    <DropdownItem key="">Tất cả danh mục</DropdownItem>,
                                    ...categories.map((category) => (
                                        <DropdownItem key={category.id}>
                                            {category.name}
                                        </DropdownItem>
                                    ))
                                ]}
                            </DropdownMenu>
                        </Dropdown>

                        {/* Clear Filters Button */}
                        {hasActiveFilters && (
                            <ButtonIconStyled
                                variant="flat"
                                color="danger"
                                size="md"
                                onPress={handleClearFilters}
                                className="min-w-fit"
                            >
                                <X size={18} />
                                <span className="ml-1">Xóa bộ lọc</span>
                            </ButtonIconStyled>
                        )}
                    </div>

                    <ButtonIconStyled
                        variant="bordered"
                        className="btn-gradient rounded-lg"
                        onPress={handleCreate}
                    >
                        <Plus />
                    </ButtonIconStyled>
                </div>

                {/* Active Filters Display */}
                {hasActiveFilters && (
                    <div className="flex flex-wrap gap-2 mb-4">
                        {filter.keyword && (
                            <Chip
                                onClose={() => handleSetFilter({ ...filter, keyword: undefined })}
                                variant="flat"
                                color="primary"
                            >
                                Tìm kiếm: {filter.keyword}
                            </Chip>
                        )}
                        {filter.categoryId && selectedCategory && (
                            <Chip
                                onClose={() =>
                                    handleSetFilter({ ...filter, categoryId: undefined })
                                }
                                variant="flat"
                                color="secondary"
                            >
                                Danh mục: {selectedCategory.name}
                            </Chip>
                        )}
                    </div>
                )}

                {/* --- TABLE --- */}
                <Suspense
                    fallback={
                        <div className="h-60 flex items-center justify-center">
                            <SpinnerStyled />
                        </div>
                    }
                >
                    <div className="bg-white rounded-2xl overflow-hidden shadow-sm border border-slate-100">
                        <ProductTable
                            products={products}
                            isLoading={isLoading || isTyping}
                            isRefetching={isRefetching}
                            onEdit={handleEdit}
                            onDelete={handleDeleteClick}
                        />
                    </div>
                </Suspense>

                {/* --- MODALS --- */}

                <ProductFormModal
                    isOpen={isOpen}
                    onOpenChange={onOpenChange}
                    product={selectedProduct} // Modal tự check: Có hàng -> Edit, Null -> Create
                />

                {/* 3. Delete */}
                <AlertModal
                    isOpen={isDeleteModalOpen}
                    onClose={() => setIsDeleteModalOpen(false)}
                    onConfirm={handleConfirmDelete}
                    header="Xác nhận xóa"
                    variant="danger"
                    isLoading={deleteMutation.isPending}
                    confirmText="Xóa vĩnh viễn"
                    body={
                        <div className="text-slate-600">
                            Bạn có chắc chắn muốn xóa sản phẩm <br />
                            <span className="font-bold text-slate-900 text-lg">
                                {selectedProduct?.name}
                            </span>{" "}
                            không?
                            <div className="mt-2 text-xs text-slate-400 bg-slate-50 p-2 rounded">
                                ⚠️ Hành động này sẽ xóa toàn bộ biến thể và lịch sử liên quan.
                            </div>
                        </div>
                    }
                />
            </div>

            {/* --- TOOLBAR --- */}
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6 bg-white p-4 rounded-2xl border border-slate-100 shadow-sm">
                <div className="w-full sm:max-w-md">
                    <InputStyled
                        placeholder="Tìm kiếm theo tên, SKU..."
                        startContent={<Search size={18} className="text-slate-400" />}
                        variant="bordered"
                        className="w-full"
                        size="md"
                    />
                </div>

                <ButtonIconStyled
                    variant="bordered"
                    className="btn-gradient rounded-lg"
                    onPress={handleCreate}
                >
                    <Plus />
                </ButtonIconStyled>
            </div>

            {/* --- TABLE --- */}
            <Suspense
                fallback={
                    <div className="h-60 flex items-center justify-center">
                        <SpinnerStyled />
                    </div>
                }
            >
                <div className="bg-white rounded-2xl overflow-hidden shadow-sm border border-slate-100">
                    <ProductTable
                        products={products}
                        isLoading={isLoading}
                        isRefetching={isRefetching}
                        onEdit={handleEdit}
                        onDelete={handleDeleteClick}
                    />
                </div>
            </Suspense>

            {/* --- MODALS --- */}

            <ProductFormModal
                isOpen={isOpen}
                onOpenChange={onOpenChange}
                product={selectedProduct} // Modal tự check: Có hàng -> Edit, Null -> Create
            />

            {/* 3. Delete */}
            <AlertModal
                isOpen={isDeleteModalOpen}
                onClose={() => setIsDeleteModalOpen(false)}
                onConfirm={handleConfirmDelete}
                header="Xác nhận xóa"
                variant="danger"
                isLoading={deleteMutation.isPending}
                confirmText="Xóa vĩnh viễn"
                body={
                    <div className="text-slate-600">
                        Bạn có chắc chắn muốn xóa sản phẩm <br />
                        <span className="font-bold text-slate-900 text-lg">
                            {selectedProduct?.name}
                        </span>{" "}
                        không?
                        <div className="mt-2 text-xs text-slate-400 bg-slate-50 p-2 rounded">
                            ⚠️ Hành động này sẽ xóa toàn bộ biến thể và lịch sử liên quan.
                        </div>
                    </div>
                }
            />
        </div>
    )
}
