"use client"
import React, { useState, useCallback, useMemo, useEffect } from "react"
import { motion, AnimatePresence } from "framer-motion"
import { useInView } from "react-intersection-observer"

import { MENU_ERRORS } from "../constants"
import { Product } from "../models/products/respone"
import { useGetAllCategories, useGetInfiniteProducts } from "../hooks"
import { AutocompleteStyled, InputStyled } from "@/components"
import { useLoginDiscloresureSingleton, useTokenStore } from "@/hooks"
import { useDisclosure, AutocompleteItem, Spinner } from "@heroui/react"
import { MapPin, Search } from "lucide-react"

import { useCartStore } from "@/features/cart/store"
import { GetAllProductParams } from "../models/products/request"
import { debouncedWrapper } from "@/utils/helpers/axiosHelper"
import { ProductCard, ProductVariantsModal, StoreSelectionModal } from "../components"

export default function MenuPage() {
    const selectedStore = useCartStore((s) => s.currentStore)
    const setStore = useCartStore((s) => s.setStore)

    const { data: categories = [] } = useGetAllCategories()
    // Search & Filter State
    const { ref, inView } = useInView()
    const [filter, setFilter] = useState<GetAllProductParams>({})
    // const [pagination, setPagination] = useState<PaginationParams>({ page: 1, pageSize: 5 })
    const { data, fetchNextPage, hasNextPage, isFetchingNextPage, isLoading } =
        useGetInfiniteProducts({
            params: filter,
            pageSize: 4
        })
    // Tự động load trang mới khi scroll đến cuối
    useEffect(() => {
        if (inView && hasNextPage && !isFetchingNextPage) {
            fetchNextPage()
        }
    }, [inView, hasNextPage, isFetchingNextPage, fetchNextPage])

    // Flatten dữ liệu từ các trang thành một mảng duy nhất
    const products = useMemo(() => data?.pages.flatMap((page) => page.items) ?? [], [data])
    const isEmpty = !products || products.length === 0

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

    const [selectedProduct, setSelectedProduct] = useState<Product | null>(null)

    const { isOpen, onOpen, onOpenChange } = useDisclosure()
    const {
        isOpen: isStoreModalOpen,
        onOpen: onOpenStoreModal,
        onOpenChange: onStoreModalOpenChange
    } = useDisclosure({ defaultOpen: false })
    const { onOpen: onOpenLogin } = useLoginDiscloresureSingleton()
    const isLogined = useTokenStore((s) => !!s.accessToken)

    const handleOpenProduct = useCallback(
        (product: Product) => {
            if (!isLogined) {
                onOpenLogin()
            } else if (!selectedStore) {
                onOpenStoreModal()
            } else {
                setSelectedProduct(product)
                onOpen()
            }
        },
        [isLogined, onOpen, onOpenLogin, onOpenStoreModal, selectedStore]
    )

    return (
        <div className="min-h-[80vh] w-7xl max-w-7xl mx-auto px-4 md:px-8">
            {/* Header Section */}
            <div className="flex flex-col xl:flex-row justify-between items-start xl:items-end gap-6 mt-8 mb-12">
                {/* Left: Title + Store Selection */}
                <div className="flex flex-col gap-4 max-w-2xl">
                    <h1 className="text-6xl md:text-7xl font-sans font-black uppercase text-slate-900 tracking-tighter leading-[0.9]">
                        CÀ MENU
                    </h1>

                    <div className="flex items-center gap-3">
                        {/* Store Selector */}
                        <div className="min-w-fit">
                            {selectedStore ? (
                                <div
                                    className="flex items-center gap-2 cursor-pointer hover:bg-default-200 transition-colors bg-default-100 px-4 py-2 rounded-full"
                                    onClick={onOpenStoreModal}
                                >
                                    <MapPin size={18} className="text-primary" />
                                    <div className="flex flex-col">
                                        <span className="font-bold text-sm text-slate-800">
                                            {selectedStore.name}
                                        </span>
                                    </div>
                                </div>
                            ) : (
                                <button
                                    onClick={onOpenStoreModal}
                                    className="flex items-center gap-2 text-sm font-semibold bg-primary text-white px-5 py-2.5 rounded-full hover:bg-primary/90 transition-shadow shadow-md hover:shadow-lg"
                                >
                                    <MapPin size={18} />
                                    Chọn cửa hàng
                                </button>
                            )}
                        </div>

                        {/* Search Input */}
                        <InputStyled
                            placeholder="Tìm kiếm theo tên, danh mục, sku"
                            startContent={<Search size={18} className="text-slate-400" />}
                            radius="full"
                            className="w-7xl"
                            classNames={{
                                inputWrapper: "bg-white shadow-sm border border-gray-200"
                            }}
                            size="md"
                            onChange={async (e) => {
                                await debouncedSearch({
                                    ...filter,
                                    keyword: e.target.value.trim()
                                })
                            }}
                            isClearable
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
                            className="bg-white border-none rounded-full"
                            radius="full"
                        >
                            {(categories ?? []).map((item) => (
                                <AutocompleteItem key={item.id}>{item.name}</AutocompleteItem>
                            ))}
                        </AutocompleteStyled>
                    </div>
                </div>
            </div>

            {isTyping ? (
                <div className="w-full text-center">
                    <Spinner />
                </div>
            ) : (
                <>
                    {/* Product Grid */}
                    <div className="flex justify-center pb-20">
                        <AnimatePresence mode="popLayout">
                            {isEmpty && !isLoading ? (
                                <motion.div
                                    key="empty"
                                    initial={{ opacity: 0, scale: 0.95 }}
                                    animate={{ opacity: 1, scale: 1 }}
                                    exit={{ opacity: 0 }}
                                    transition={{ duration: 0.4 }}
                                    className="text-gray-500 mt-10 text-lg font-medium text-center w-full"
                                >
                                    {!selectedStore
                                        ? "Vui lòng chọn cửa hàng để xem menu"
                                        : MENU_ERRORS.NOT_HAVE_PRODUCT}
                                </motion.div>
                            ) : (
                                <motion.div
                                    key="grid"
                                    initial={{ opacity: 0 }}
                                    animate={{ opacity: 1 }}
                                    // exit={{ opacity: 0 }} // Removing exit animation for the gri container to prevent flashing
                                    className="w-full grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-x-8 gap-y-12"
                                >
                                    {products.map((product) => (
                                        <motion.div // Add layout prop for smooth reordering
                                            key={product.id}
                                            initial={{ opacity: 0, y: 20 }}
                                            animate={{ opacity: 1, y: 0 }}
                                            exit={{ opacity: 0, scale: 0.9 }}
                                            transition={{ duration: 0.3 }}
                                        >
                                            <ProductCard
                                                key={product.id}
                                                product={product}
                                                onClick={() => handleOpenProduct(product)}
                                            />
                                        </motion.div>
                                    ))}
                                </motion.div>
                            )}
                        </AnimatePresence>
                    </div>
                    {/* Điểm neo để phát hiện scroll xuống cuối */}
                    <div ref={ref} className="h-20 flex items-center text-md justify-center">
                        {isFetchingNextPage ? (
                            <Spinner label="Đang lấy thêm cà phê..." color="primary" />
                        ) : hasNextPage ? (
                            <p className="text-slate-400 italic">Cuộn tiếp để xem thêm sản phẩm</p>
                        ) : (
                            !isEmpty && (
                                <p className="text-slate-400 font-medium">
                                    Bạn đã xem hết menu rồi!
                                </p>
                            )
                        )}
                    </div>
                </>
            )}

            <ProductVariantsModal
                product={selectedProduct}
                isOpen={isOpen}
                onOpenChange={onOpenChange}
                storeId={selectedStore?.id}
            />

            <StoreSelectionModal
                isOpen={isStoreModalOpen}
                onOpenChange={onStoreModalOpenChange}
                onStoreSelect={(store) => {
                    setStore(store)
                }}
            />
        </div>
    )
}
