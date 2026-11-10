"use client"

import { useImageUploadModal } from "@/hooks"
import { useUploadProductImage } from "@/features/products/hooks"
import { DropdownStyled, ImagePreviewModal, ImageStyled, ImageUploaderModal } from "@/components"
import { Camera, Eye } from "lucide-react"
import { DropdownItem, DropdownMenu, DropdownTrigger, useDisclosure } from "@heroui/react"
import { useMemo } from "react"

type Props = {
    productId: string
    currentImageUrl?: string
    onSuccess?: () => void
    isReadOnly: boolean
}

export function ProductImageUploader({ productId, currentImageUrl, onSuccess, isReadOnly }: Props) {
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

    const disabledImageActions = useMemo(() => {
        const keys: string[] = []
        if (isReadOnly) keys.push("upload")
        if (!currentImageUrl) keys.push("view")
        return keys
    }, [isReadOnly, currentImageUrl])

    const menuItems = useMemo(() => {
        const baseItems = [{ key: "view", label: "Xem ảnh", icon: <Eye size={20} /> }]

        if (!isReadOnly) {
            baseItems.push({ key: "upload", label: "Thay đổi ảnh", icon: <Camera size={20} /> })
        }

        return baseItems
    }, [isReadOnly])

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
                    items={menuItems}
                    disabledKeys={disabledImageActions}
                >
                    {(item) => (
                        <DropdownItem
                            key={item.key}
                            startContent={item.icon}
                            className={item.key === "upload" ? "text-primary" : ""}
                        >
                            {item.label}
                        </DropdownItem>
                    )}
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
