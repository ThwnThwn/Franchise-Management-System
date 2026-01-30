"use client"

import React, { Suspense, useState } from "react"
import { Coffee, Plus, Search } from "lucide-react"
import { useDisclosure } from "@heroui/react"
import { useDeleteProduct, useGetAllProducts } from "../hooks"
import { Product } from "../models/products/respone"
import { AlertModal, ButtonIconStyled, InputStyled, SpinnerStyled } from "@/components"
import { ProductTable } from "../components/ProductTable"
import { ProductFormModal } from "../components"

// Import Components & Hooks

export default function ProductManagementPage() {
    // Fetch Data
    const { data: productsData, isLoading, isRefetching } = useGetAllProducts()
    const products = productsData?.products || []

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

    return (
        <div className="min-h-screen p-4 md:p-6 lg:p-8 bg-slate-50/30">
            <div className="max-w-7xl mx-auto">
                {/* --- HEADER --- */}
                <div className="mb-8">
                    <div className="flex items-center gap-3 mb-2">
                        <div className="w-12 h-12 rounded-xl bg-linear-to-br from-primary to-secondary flex items-center justify-center">
                            <Coffee className="w-6 h-6 text-white" />
                        </div>
                        <div>
                            <h1 className="text-3xl lg:text-4xl font-bold text-slate-900 tracking-tight">
                                Quản lý sản phẩm
                            </h1>
                            <p className="text-slate-500 mt-1 text-sm font-medium">
                                Quản lý danh sách, giá cả và danh mục
                            </p>
                        </div>
                    </div>
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
        </div>
    )
}
