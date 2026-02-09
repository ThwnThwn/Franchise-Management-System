"use client"

import React from "react"
import { useImageUploadModal } from "@/hooks"
import { useUploadProductImage } from "../../hooks"
import { DropdownStyled, ImageStyled, ImageUploaderModal, ModalStyled } from "@/components"
import { Camera, Eye } from "lucide-react"
import {
    DropdownItem,
    DropdownMenu,
    DropdownTrigger,
    ModalBody,
    ModalContent,
    useDisclosure
} from "@heroui/react"
import Image from "next/image"

type Props = {
    productId: string
    currentImageUrl?: string
    onSuccess?: () => void
}

export function ProductImageUploader({ productId, currentImageUrl, onSuccess }: Props) {
    const {
        isOpen: isPreviewOpen,
        onOpen: onOpenPreview,
        onOpenChange: onPreviewChange
    } = useDisclosure()

    const {
        isOpen: isDropdownOpen,
        onOpenChange: onDropdownOpenChange,
        onClose: onDropdownClose
    } = useDisclosure()

    const { imgSrc, setImgSrc, isOpen, onOpen, onOpenChange, onClose } = useImageUploadModal({
        onBeforeOpenModal: onDropdownClose
    })

    const uploadMutation = useUploadProductImage({ id: productId, onSuccess: onSuccess })

    return (
        <div className="relative group w-40 h-40 rounded-xl overflow-hidden border border-gray-200 shadow-sm bg-gray-50">
            {/* Ảnh hiển thị hiện tại */}
            <DropdownStyled
                placement="bottom" // Đổi thành bottom để menu không che mất ảnh khi mở ra
                classNames={{ content: "min-w-[150px]" }} // Chỉnh lại min-width cho đẹp
                isOpen={isDropdownOpen}
                onOpenChange={onDropdownOpenChange}
            >
                <DropdownTrigger className="w-full h-full cursor-pointer">
                    {/* Bọc div để tránh lỗi ref từ Trigger truyền vào ImageStyled */}
                    <div className="w-full h-full">
                        <ImageStyled
                            src={currentImageUrl}
                            alt="Product Thumbnail"
                            className="w-40 h-40 object-cover"
                        />
                    </div>
                </DropdownTrigger>

                <DropdownMenu
                    variant="flat"
                    aria-label="Product Image Actions"
                    onAction={(key) => {
                        if (key === "view") onOpenPreview()
                        if (key === "upload") onOpen()
                    }}
                    // Fix lỗi type cho disabledKeys: Phải là mảng (Iterable)
                    disabledKeys={!currentImageUrl ? ["view"] : []}
                >
                    <DropdownItem
                        key="view"
                        startContent={<Eye size={20} className="text-primary" />}
                    >
                        Xem ảnh
                    </DropdownItem>

                    <DropdownItem
                        key="upload"
                        startContent={<Camera size={20} className="text-primary" />}
                        className="text-primary font-medium"
                    >
                        Thay đổi ảnh
                    </DropdownItem>
                </DropdownMenu>
            </DropdownStyled>

            {/* Modal Preview Ảnh Lớn */}
            {currentImageUrl && (
                <ModalStyled
                    isOpen={isPreviewOpen}
                    onOpenChange={onPreviewChange}
                    size="3xl"
                    placement="center"
                    classNames={{
                        body: "p-0",
                        backdrop: "bg-black/80", // Nền tối hơn cho giống chế độ xem ảnh
                        base: "bg-transparent shadow-none" // Bỏ khung trắng của modal
                    }}
                    isDismissable
                    hideCloseButton
                >
                    <ModalContent>
                        <ModalBody className="flex items-center justify-center w-full">
                            {/* Dùng fill + object-contain để ảnh tự co giãn vừa khung hình */}
                            <Image
                                src={currentImageUrl}
                                alt="Full Preview"
                                className="object-contain rounded-md"
                                width={400}
                                height={300}
                            />
                        </ModalBody>
                    </ModalContent>
                </ModalStyled>
            )}

            {/* Modal Upload */}
            <ImageUploaderModal
                isOpen={isOpen}
                onOpenChange={onOpenChange}
                onClose={onClose}
                imgSrc={imgSrc}
                setImgSrc={setImgSrc}
                uploadFn={uploadMutation.mutateAsync}
                isUploadPending={uploadMutation.isPending}
                label="Cập nhật ảnh sản phẩm"
                // aspect={4 / 3}
                cropShape="rect"
                cropSize={{ width: 400, height: 300 }}
            />
        </div>
    )
}
