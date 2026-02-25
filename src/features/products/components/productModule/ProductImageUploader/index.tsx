"use client"

import React from "react"
import { useImageUploadModal, useTokenStore } from "@/hooks"
import { useUploadProductImage } from "@/features/products/hooks"
import { DropdownStyled, ImagePreviewModal, ImageStyled, ImageUploaderModal } from "@/components"
import { Camera, Eye } from "lucide-react"
import { DropdownItem, DropdownMenu, DropdownTrigger, useDisclosure } from "@heroui/react"
import { RoleSlug } from "@/constants/enums"

type Props = {
    productId: string
    currentImageUrl?: string
    onSuccess?: () => void
}

export function ProductImageUploader({ productId, currentImageUrl, onSuccess }: Props) {
    const isReadOnly = useTokenStore((s) => s.roleSlug !== RoleSlug.Admin)

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
                    disabledKeys={isReadOnly ? ["upload"] : !currentImageUrl ? ["view"] : []}
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
                <ImagePreviewModal
                    isOpen={isPreviewOpen}
                    onOpenChange={onPreviewChange}
                    imgUrl={currentImageUrl}
                    width={400}
                    height={400}
                />
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
                cropSize={{ width: 400, height: 400 }}
            />
        </div>
    )
}
