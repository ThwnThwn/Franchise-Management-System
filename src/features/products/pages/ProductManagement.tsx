"use client"

import React, { Suspense, useCallback, useMemo, useState } from "react"
import { Coffee, Plus, Search } from "lucide-react"
import { AutocompleteItem } from "@heroui/react"
import { useDeleteProduct, useGetAllProducts } from "../hooks"
import { useGetAllCategories } from "../hooks/useCategory"
import { Product } from "../models/products/respone"
import {
    AlertModal,
    AutocompleteStyled,
    ButtonIconStyled,
    InputStyled,
    PaginationStyled,
    SpinnerStyled
} from "@/components"
import { GetAllProductParams } from "@/features/products/models/products/request"
import { debouncedWrapper } from "@/utils/helpers/axiosHelper"
import { PaginationParams } from "@/models/common/request"
import { DashboardHeader } from "@/features/dashboard/components/general/DashboardHeader"
import { ProductTable } from "@/features/products/components"
import { useRouter } from "next/navigation"
import { PRODUCTS_ROUTES } from "@/features/products/constants/routes"

// Import Components & Hooks

export default function ProductManagementPage() {
    const router = useRouter()

    const { data: categories = [] } = useGetAllCategories()
    // Search & Filter State
    const [filter, setFilter] = useState<GetAllProductParams>({})
    const [pagination, setPagination] = useState<PaginationParams>({ page: 1, pageSize: 10 })
    const { data, isLoading, isRefetching } = useGetAllProducts({
        params: filter,
        pagination,
        enabled: true
    })
    const products = data?.items ?? []

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
                1000,
                () => setIsTyping(true),
                () => setIsTyping(false)
            ),
        [handleSetFilter]
    )

    // State & Hooks quản lý Modal
    const [selectedProduct, setSelectedProduct] = useState<Product | null>(null)

    // Chỉ cần 1 biến state cho modal
    // const { isOpen, onOpen, onOpenChange } = useDisclosure()

    // Handler: Mở để Tạo mới
    const handleCreate = () => {
        // setSelectedProduct(null) // Quan trọng: set null để modal biết là Create
        // onOpen()
        router.push(PRODUCTS_ROUTES.CREATE)
    }

    // Handler: Mở để Update
    const handleEdit = (productId: string) => {
        // setSelectedProduct(product) // Truyền product vào để modal biết là Edit
        // onOpen()
        router.push(PRODUCTS_ROUTES.EDIT(productId))
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

    return (
        <div className="max-w-7xl mb-8 mx-auto min-h-screen p-4 md:p-6 lg:p-8 bg-slate-50/30">
            {/* --- HEADER --- */}
            <DashboardHeader
                icon={<Coffee className="w-6 h-6 text-white" />}
                title="Quản lý sản phẩm"
                description="Quản lý danh sách, giá cả và danh mục"
            />

            {/* --- TOOLBAR --- */}
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-3 bg-white p-4 rounded-2xl border border-slate-100 shadow-sm">
                <InputStyled
                    placeholder="Tìm kiếm theo tên, danh mục, sku"
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
                <AutocompleteStyled
                    placeholder={"Danh mục"}
                    items={categories}
                    selectedKey={filter.categoryId}
                    onSelectionChange={(id) => {
                        setFilter((prev) => {
                            return {
                                ...prev,
                                categoryId: id ? (id as string) : undefined
                            }
                        })
                    }}
                    className="md:w-100"
                >
                    {(categories ?? []).map((item) => (
                        <AutocompleteItem key={item.id}>{item.name}</AutocompleteItem>
                    ))}
                </AutocompleteStyled>

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
            {(data?.items || []).length > 0 && !isLoading && !isTyping && (
                <PaginationStyled
                    page={pagination.page ?? 1}
                    total={data?.totalPages ?? 10}
                    onChange={(page: number) =>
                        setPagination((prev) => {
                            return {
                                ...prev,
                                page: page
                            }
                        })
                    }
                />
            )}

            <>
                {/* --- MODALS --- */}
                {/* <ProductFormModal
                    isOpen={isOpen}
                    onOpenChange={onOpenChange}
                    product={selectedProduct} // Modal tự check: Có hàng -> Edit, Null -> Create
                /> */}

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
            </>
        </div>
    )
}
