"use client"

import React from "react"
import { confirmDialog, useImageUploadModal } from "@/hooks"
import { DropdownStyled, ImagePreviewModal, ImageStyled, ImageUploaderModal } from "@/components"
import { Camera, Eye, Trash } from "lucide-react"
import { DropdownItem, DropdownMenu, DropdownTrigger, useDisclosure } from "@heroui/react"
import { useDeleteMyAvatar, useUploadMyAvatar } from "@/features/profile/hooks"
import { DEFAULT_AVATAR_URL } from "@/constants/constants"

type Props = {
    currentImageUrl?: string
    onSuccess?: () => void
}

export function AvatarImageUploader({ currentImageUrl, onSuccess }: Props) {
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

    const uploadMutation = useUploadMyAvatar({ onSuccess: onSuccess })

    const deleteMutation = useDeleteMyAvatar({ onSuccess: onSuccess })
    const handleDelete = async () => {
        const isConfirm = await confirmDialog({
            title: "Xoá ảnh đại diện",
            variant: "danger"
        })

        if (isConfirm) {
            deleteMutation.mutate()
        }
    }

    return (
        <div className="relative group w-40 h-40 rounded-full overflow-hidden border border-gray-200 shadow-sm bg-gray-50">
            {/* Ảnh hiển thị hiện tại */}
            <DropdownStyled
                placement="right-end"
                classNames={{ content: "min-w-[150px]" }}
                isOpen={isDropdownOpen}
                onOpenChange={onDropdownOpenChange}
            >
                <DropdownTrigger className="w-full h-full cursor-pointer">
                    <div className="w-full h-full">
                        <ImageStyled
                            src={currentImageUrl || DEFAULT_AVATAR_URL}
                            alt="Avatar"
                            className="w-40 h-40 object-cover rounded-full"
                        />
                    </div>
                </DropdownTrigger>

                <DropdownMenu
                    variant="flat"
                    aria-label="Avatar Actions"
                    onAction={(key) => {
                        if (key === "view") onOpenPreview()
                        if (key === "upload") onOpen()
                        if (key === "delete") handleDelete()
                    }}
                    disabledKeys={!currentImageUrl ? ["view", "delete"] : []}
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
                        Tải ảnh lên
                    </DropdownItem>

                    <DropdownItem
                        key="delete"
                        startContent={<Trash size={20} className="text-danger" />}
                        className="text-danger font-medium"
                    >
                        Xoá ảnh
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
                label="Cập nhật ảnh đại diện"
                cropShape="round"
                cropSize={{ width: 400, height: 400 }}
            />
        </div>
    )
}
