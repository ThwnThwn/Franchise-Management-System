"use client"
import React from "react"
import { ModalContent, ModalHeader, ModalBody, ModalFooter } from "@heroui/react"
import StoreList from "@/features/stores/components/StoreList"
import { StoreDetailDto } from "@/features/stores/models/schema/response"
import { useUserLocation } from "@/features/stores/hooks/useUserLocation"
import { ModalStyled } from "@/components"
import { useGetNearbyStores } from "@/features/stores"
import Link from "next/link"

type StoreSelectionModalProps = {
    isOpen: boolean
    onOpenChange: (isOpen: boolean) => void // Fixed type signature
    onStoreSelect: (store: StoreDetailDto) => void
}

export default function StoreSelectionModal({
    isOpen,
    onOpenChange,
    onStoreSelect
}: StoreSelectionModalProps) {
    // const [keyword, setKeyword] = useState("")
    // const { data, loading } = useStores({ page: 1, pageSize: 12, keyword })

    const LIMIT = 3
    const RADIUS_KM = 6
    const { latitude, longitude } = useUserLocation()
    const enabled = latitude !== null && longitude !== null

    const { data, isLoading } = useGetNearbyStores({
        params: {
            userLat: latitude || 0,
            userLong: longitude || 0,
            limit: LIMIT,
            radiusKm: RADIUS_KM
        },
        enabled: enabled
    })

    return (
        <ModalStyled
            isOpen={isOpen}
            onOpenChange={onOpenChange}
            size="3xl"
            scrollBehavior="inside"
            isDismissable
        >
            <ModalContent>
                {(onClose) => (
                    <>
                        <ModalHeader className="flex flex-col gap-1">
                            <h3>Chọn cửa hàng</h3>
                            <div className="text-sm italic text-secondary font-normal">
                                Dưới đây là {data?.totalCount || 0} cửa hàng gần nhất trong khu vực{" "}
                                {RADIUS_KM} km quanh bạn
                            </div>
                        </ModalHeader>
                        <ModalBody className="pb-6">
                            {enabled ? (
                                <StoreList
                                    stores={data?.items ?? []}
                                    loading={isLoading}
                                    onStoreClick={(store) => {
                                        onStoreSelect(store)
                                        onClose()
                                    }}
                                    userLocation={
                                        latitude && longitude ? { latitude, longitude } : null
                                    }
                                />
                            ) : (
                                "Vui lòng cho phép tính năng GPS để xem các cửa hàng xung quanh bạn"
                            )}
                        </ModalBody>
                        <ModalFooter className="flex justify-between items-center pt-2">
                            <span className="text-sm text-gray-500">
                                Không tìm thấy cửa hàng ưng ý?
                            </span>
                            <Link
                                href="/stores"
                                target="_blank"
                                rel="noopener noreferrer"
                                className="text-primary hover:underline text-sm font-medium"
                            >
                                Xem tất cả cửa hàng →
                            </Link>
                        </ModalFooter>
                    </>
                )}
            </ModalContent>
        </ModalStyled>
    )
}
