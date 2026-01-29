"use client"
import React, { useState } from "react"
import { Modal, ModalContent, ModalHeader, ModalBody, Input } from "@heroui/react"
import { useStores } from "@/features/stores/hooks/useStores"
import StoreList from "@/features/stores/components/StoreList"
import { StoreDetailDto } from "@/features/stores/models/schema/response"
import { useUserLocation } from "@/features/stores/hooks/useUserLocation"

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
    const [keyword, setKeyword] = useState("")
    const { data, loading } = useStores({ page: 1, pageSize: 12, keyword })
    const { latitude, longitude } = useUserLocation()

    return (
        <Modal
            isOpen={isOpen}
            onOpenChange={onOpenChange}
            size="5xl"
            scrollBehavior="inside"
            isDismissable={false}
        >
            <ModalContent>
                {(onClose) => (
                    <>
                        <ModalHeader className="flex flex-col gap-1">
                            <h3>Chọn cửa hàng</h3>
                            <Input
                                placeholder="Tìm kiếm cửa hàng..."
                                value={keyword}
                                onValueChange={setKeyword}
                                isClearable
                                onClear={() => setKeyword("")}
                                className="max-w-xs mt-2"
                            />
                        </ModalHeader>
                        <ModalBody className="pb-6">
                            <StoreList
                                stores={data?.items ?? []}
                                loading={loading}
                                onStoreClick={(store) => {
                                    onStoreSelect(store)
                                    onClose()
                                }}
                                userLocation={latitude && longitude ? { latitude, longitude } : null}
                            />
                        </ModalBody>
                    </>
                )}
            </ModalContent>
        </Modal>
    )
}
