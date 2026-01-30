"use client"

import React, { useState, useEffect, useMemo } from "react"
import { useStores, useUserLocation } from "@/features/stores/hooks"
import StoreList from "@/features/stores/components/StoreList"
import StoreDetailModal from "@/features/stores/components/StoreDetailModal"
import Pagination from "@/components/shared/Pagination"
import { StoreDetailDto } from "@/features/stores/models/schema/response"

export default function StoresPage() {
    const [currentPage, setCurrentPage] = useState(1)
    const [keyword, setKeyword] = useState("")
    const [searchInput, setSearchInput] = useState("")
    const [selectedStore, setSelectedStore] = useState<StoreDetailDto | null>(null)
    const [isModalOpen, setIsModalOpen] = useState(false)

    const pageSize = 9
    const { latitude, longitude, loading: locationLoading } = useUserLocation()

    const { data, loading, error } = useStores({
        page: currentPage,
        pageSize,
        keyword
    })

    // Sort stores by distance if GPS is available, otherwise keep original order (backend sorted by newest)
    const sortedStores = useMemo(() => {
        if (!data?.items) return []

        if (latitude && longitude) {
            // Calculate distance and sort
            const storesWithDistance = data.items.map((store) => {
                if (!store.latitude || !store.longitude) {
                    return { store, distance: Infinity }
                }

                const R = 6371 // Earth radius in km
                const dLat = ((store.latitude - latitude) * Math.PI) / 180
                const dLon = ((store.longitude - longitude) * Math.PI) / 180
                const a =
                    Math.sin(dLat / 2) * Math.sin(dLat / 2) +
                    Math.cos((latitude * Math.PI) / 180) *
                        Math.cos((store.latitude * Math.PI) / 180) *
                        Math.sin(dLon / 2) *
                        Math.sin(dLon / 2)
                const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a))
                const distance = R * c

                return { store, distance }
            })

            return storesWithDistance
                .sort((a, b) => a.distance - b.distance)
                .map((item) => item.store)
        }

        // No GPS, return as is (backend already sorted by newest)
        return data.items
    }, [data?.items, latitude, longitude])

    const handleSearch = () => {
        setKeyword(searchInput)
        setCurrentPage(1)
    }

    const handlePageChange = (page: number) => {
        setCurrentPage(page)
        window.scrollTo({ top: 0, behavior: "smooth" })
    }

    const handleStoreClick = (store: StoreDetailDto) => {
        setSelectedStore(store)
        setIsModalOpen(true)
    }

    const handleCloseModal = () => {
        setIsModalOpen(false)
        setSelectedStore(null)
    }

    return (
        <div className="min-h-screen w-200">
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
                    {latitude && longitude && !locationLoading && (
                        <p className="text-sm text-green-600 mt-2">Những cửa hàng gần bạn</p>
                    )}
                    {!latitude && !locationLoading && (
                        <p className="text-sm text-gray-500 mt-2">
                            Sắp xếp theo mới nhất (cho phép GPS để xem cửa hàng gần nhất)
                        </p>
                    )}
                </div>
            </div>

            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                {error && (
                    <div className="bg-red-50 text-red-600 p-4 rounded-lg mb-6">
                        Có lỗi xảy ra: {error}
                    </div>
                )}

                <StoreList
                    stores={sortedStores}
                    loading={loading}
                    onStoreClick={handleStoreClick}
                    userLocation={latitude && longitude ? { latitude, longitude } : null}
                />

                {data && (
                    <Pagination
                        currentPage={currentPage}
                        totalPages={data.totalPages}
                        onPageChange={handlePageChange}
                    />
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
