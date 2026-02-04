"use client"

import React, { useState, useMemo, useEffect } from "react"
import { useGetStores, useUserLocation } from "@/features/stores/hooks"
import StoreList from "@/features/stores/components/StoreList"
import StoreDetailModal from "@/features/stores/components/StoreDetailModal"
import { StoreDetailDto } from "@/features/stores/models/schema/response"
import { calculateDistance } from "@/utils/helpers/locationHelper"
import { PaginationStyled } from "@/components"
import { PaginationParams } from "@/models/common/request"
import { BackendError } from "@/models/common/respone"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"

export default function StoresPage() {
    const [keyword, setKeyword] = useState("")
    const [searchInput, setSearchInput] = useState("")
    const [selectedStore, setSelectedStore] = useState<StoreDetailDto | null>(null)
    const [isModalOpen, setIsModalOpen] = useState(false)

    const [pagination, setPagination] = useState<PaginationParams>({ page: 1, pageSize: 10 })

    const { latitude, longitude, loading: locationLoading } = useUserLocation()

    const { data, isLoading, error } = useGetStores({
        page: pagination.page,
        pageSize: pagination.pageSize,
        keyword
    })

    // Sort stores by distance if GPS is available, otherwise keep original order (backend sorted by newest)
    const sortedStores = useMemo(() => {
        // 1. Nếu không có data, trả về mảng rỗng
        if (!data) return []

        // 2. Nếu không có GPS của user, trả về danh sách mặc định (Backend đã sort)
        if (!latitude || !longitude) {
            return data.items
        }

        // 3. Có GPS -> Tính toán và Sort
        return data.items
            .map((store) => {
                // Nếu store thiếu tọa độ, đẩy xuống cuối cùng (Infinity)
                if (!store.latitude || !store.longitude) {
                    return { store, distance: Infinity }
                }

                // Gọi hàm Utils ở đây 👇
                const distance = calculateDistance(
                    latitude,
                    longitude,
                    store.latitude,
                    store.longitude
                )

                return { store, distance }
            })
            .sort((a, b) => a.distance - b.distance) // Sort tăng dần theo khoảng cách
            .map((item) => item.store) // Trả về lại object store nguyên bản
    }, [data, latitude, longitude])

    const handleSearch = () => {
        setKeyword(searchInput)
        setPagination((prev) => {
            return {
                ...prev,
                page: 1
            }
        })
    }

    const handleStoreClick = (store: StoreDetailDto) => {
        setSelectedStore(store)
        setIsModalOpen(true)
    }

    const handleCloseModal = () => {
        setIsModalOpen(false)
        setSelectedStore(null)
    }

    useEffect(() => {
        if (error) {
            const backendErr = error as BackendError
            addToast({
                title: backendErr.title || STRINGS.COMMON.ERROR,
                description: backendErr.detail,
                color: "danger"
            })
        }
    })

    return (
        <div className="min-h-screen">
            <div className="bg-white shadow-sm rounded-3xl">
                <div className="max-w-7xl px-4 sm:px-6 lg:px-8 py-6">
                    <h1 className="text-3xl font-bold text-gray-900 mb-6">Danh sách Cửa hàng</h1>

                    <div className="flex flex-col sm:flex-row gap-4">
                        <div className="flex-1">
                            <div className="relative">
                                <input
                                    type="text"
                                    placeholder="Tìm kiếm cửa hàng..."
                                    value={searchInput}
                                    onChange={(e) => setSearchInput(e.target.value)}
                                    onKeyPress={(e) => e.key === "Enter" && handleSearch()}
                                    className="w-full px-4 py-3 pl-10 pr-4 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-amber-500"
                                />
                                <svg
                                    xmlns="http://www.w3.org/2000/svg"
                                    className="h-5 w-5 text-gray-400 absolute left-3 top-1/2 transform -translate-y-1/2"
                                    fill="none"
                                    viewBox="0 0 24 24"
                                    stroke="currentColor"
                                >
                                    <path
                                        strokeLinecap="round"
                                        strokeLinejoin="round"
                                        strokeWidth={2}
                                        d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
                                    />
                                </svg>
                            </div>
                        </div>
                        <button
                            onClick={handleSearch}
                            className="px-6 py-3 bg-[#5D4037] text-white rounded-lg hover:bg-[#4E342E] transition-colors font-semibold shadow-md"
                        >
                            Tìm kiếm
                        </button>
                    </div>

                    {locationLoading && (
                        <p className="text-sm text-gray-500 mt-2">Đang lấy vị trí GPS...</p>
                    )}
                    {/* {latitude && longitude && !locationLoading && (
                        <p className="text-sm text-green-600 mt-2">Những cửa hàng gần bạn</p>
                    )} */}
                    {!latitude && !locationLoading && (
                        <p className="text-sm text-gray-500 mt-2">
                            Sắp xếp theo mới nhất (cho phép GPS để xem cửa hàng gần nhất)
                        </p>
                    )}
                </div>
            </div>

            <div className="max-w-7xl mx-auto py-8">
                <StoreList
                    stores={sortedStores}
                    loading={isLoading}
                    onStoreClick={handleStoreClick}
                    userLocation={latitude && longitude ? { latitude, longitude } : null}
                />

                {(data?.items || []).length > 0 && !isLoading && (
                    <div className="mt-6 flex justify-center">
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
                    </div>
                )}
            </div>

            <StoreDetailModal
                store={selectedStore}
                isOpen={isModalOpen}
                onClose={handleCloseModal}
            />
        </div>
    )
}
