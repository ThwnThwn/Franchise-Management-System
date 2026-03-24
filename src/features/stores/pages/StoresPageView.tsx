"use client"

import { InputStyled, PaginationStyled } from "@/components"
import { STRINGS } from "@/constants/strings"
import StoreDetailModal from "@/features/stores/components/StoreDetailModal"
import StoreList from "@/features/stores/components/StoreList"
import { useGetStores, useUserLocation } from "@/features/stores/hooks"
import { StoreDetailDto } from "@/features/stores/models/schema/response"
import { PaginationParams } from "@/models/common/request"
import { BackendError } from "@/models/common/respone"
import { addToast } from "@heroui/react"
import { Search } from "lucide-react"
import { useEffect, useState } from "react"
import { GetStoresParams } from "../models/schema/request"

export default function StoresPageView() {
    const [searchInput, setSearchInput] = useState("")
    const [selectedStore, setSelectedStore] = useState<StoreDetailDto | null>(null)
    const [isModalOpen, setIsModalOpen] = useState(false)

    const [filter, setFilter] = useState<GetStoresParams>({})
    const [pagination, setPagination] = useState<PaginationParams>({ page: 1, pageSize: 10 })
    const { data, isLoading, error } = useGetStores({
        pagination: {
            page: pagination.page,
            pageSize: pagination.pageSize
        },
        params: filter
    })

    const handleSearch = () => {
        setFilter({ keyword: searchInput })
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

    const { latitude, longitude, loading: locationLoading } = useUserLocation()

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
            <div className="flex flex-col gap-4 w-3xl max-w-3xl mt-3 px-4">
                <h1 className="text-3xl md:text-5xl font-sans font-black uppercase text-slate-900 tracking-tighter leading-[0.9]">
                    Danh sách Cửa hàng
                </h1>

                <div className="flex flex-col sm:flex-row gap-4">
                    <div className="flex-1">
                        <div className="relative">
                            <InputStyled
                                placeholder="Tìm kiếm cửa hàng"
                                startContent={<Search size={18} className="text-slate-400" />}
                                radius="full"
                                classNames={{
                                    inputWrapper: "bg-white shadow-sm border border-gray-200"
                                }}
                                size="lg"
                                value={searchInput}
                                onChange={(e) => setSearchInput(e.target.value)}
                                onKeyUp={(e) => e.key === "Enter" && handleSearch()}
                                isClearable
                            />
                        </div>
                    </div>
                </div>

                {/* {locationLoading && (
                    <p className="text-sm text-gray-500 mt-2">Đang lấy vị trí GPS...</p>
                )} */}
                {/* {latitude && longitude && !locationLoading && (
                        <p className="text-sm text-green-600 mt-2">Những cửa hàng gần bạn</p>
                    )} */}
                {!latitude && !locationLoading && (
                    <p className="text-sm text-gray-500 mt-2">
                        Cho phép GPS để xem cửa hàng gần nhất
                    </p>
                )}
            </div>

            <div className="max-w-7xl mx-auto py-8">
                <StoreList
                    stores={data?.items ?? []}
                    loading={isLoading}
                    onStoreClick={handleStoreClick}
                    userLocation={latitude && longitude ? { latitude, longitude } : null}
                />

                {(data?.items || []).length > 0 && !isLoading && (
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
            </div>

            <StoreDetailModal
                store={selectedStore}
                isOpen={isModalOpen}
                onClose={handleCloseModal}
            />
        </div>
    )
}
