"use client"

import React, { useCallback, useState } from "react"
import { ModalStyled, ButtonStyled, ImageCropper, FileDropZone } from "@/components/"
import { ModalContent, ModalBody, ModalHeader } from "@heroui/react"
import { getCroppedImage } from "@/utils/helpers/image"

type ImageUploaderModalProps = {
    isOpen: boolean
    onOpenChange: () => void
    onClose: () => void
    imgSrc: string | null
    setImgSrc: (src: string | null) => void
    setDisplayImg?: (src: string | undefined) => void
    // aspect?: number
    cropShape?: "rect" | "round"
    cropSize: { width: number; height: number }
    label?: string
    uploadFn: (formData: FormData) => Promise<any>
    isUploadPending: boolean
}

export function ImageUploaderModal({
    isOpen,
    onOpenChange,
    onClose,
    imgSrc,
    setImgSrc,
    uploadFn,
    setDisplayImg,
    isUploadPending,
    // aspect = 1,
    cropShape = "rect",
    cropSize = { width: 400, height: 300 },
    label
}: ImageUploaderModalProps) {
    const aspect = cropSize.width / cropSize.height
    const [croppedAreaPixels, setCroppedAreaPixels] = useState<any>(null)

    const handleOpenChange = () => {
        // Nếu modal đang mở và sắp đóng -> Reset state
        if (isOpen) {
            setCroppedAreaPixels(null)
            setImgSrc(null) // Nếu bạn muốn reset luôn ảnh
            onClose() // Gọi hàm đóng gốc
        } else {
            // Nếu đang đóng và sắp mở
            onOpenChange()
        }
    }
    // Xử lý khi Drop file thành công
    const onDropFile = useCallback(
        (file: File) => {
            const reader = new FileReader()
            reader.addEventListener("load", () => {
                setImgSrc(reader.result as string)
            })
            reader.readAsDataURL(file)
        },
        [setImgSrc]
    )

    const handleUpload = useCallback(async () => {
        if (!imgSrc || !croppedAreaPixels) return

        const blob = await getCroppedImage(imgSrc, croppedAreaPixels)
        const formData = new FormData()

        // Key là "image" cho khớp với Backend Go
        formData.append("image", blob, "image.jpg")

        const res = await uploadFn(formData)
        if (!!res && typeof res.img === "string" && !!setDisplayImg) {
            setDisplayImg(res.img)
        }

        setImgSrc(null)
        onClose()
    }, [croppedAreaPixels, imgSrc, onClose, setDisplayImg, setImgSrc, uploadFn])

    // Nút Hủy hoặc Chọn lại ảnh khác
    const handleCancel = () => {
        if (imgSrc) {
            // Nếu đang có ảnh thì nút Hủy sẽ reset về màn hình DropZone
            setImgSrc(null)
        } else {
            // Nếu đang ở DropZone thì đóng modal
            onClose()
        }
    }

    return (
        <ModalStyled
            isOpen={isOpen}
            onOpenChange={() => {
                if (!isUploadPending) handleOpenChange()
            }}
            // isDismissable={!isUploadPending}
        >
            <ModalContent className="w-full p-4">
                <ModalHeader className="px-3 py-2 self-center text-xl font-bold">
                    {label || "Tải ảnh"}
                </ModalHeader>
                <ModalBody>
                    {/* LOGIC CHUYỂN ĐỔI VIEW */}
                    {!imgSrc ? (
                        // VIEW 1: DROP ZONE
                        <div className="py-4">
                            <FileDropZone onFileSelect={onDropFile} />
                        </div>
                    ) : (
                        // VIEW 2: CROPPER
                        <div className="flex flex-col items-center gap-6 animate-in fade-in zoom-in duration-300">
                            <ImageCropper
                                imgSrc={imgSrc}
                                onCropComplete={setCroppedAreaPixels}
                                aspect={aspect}
                                cropShape={cropShape}
                                cropSize={cropSize}
                                className="rounded-lg shadow-lg border border-gray-200"
                            />

                            <div className="flex gap-3 w-full justify-center">
                                <ButtonStyled onPress={handleCancel} isDisabled={isUploadPending}>
                                    Chọn lại
                                </ButtonStyled>

                                <ButtonStyled
                                    color="primary"
                                    onPress={handleUpload}
                                    isLoading={isUploadPending}
                                    className="px-8 font-semibold"
                                >
                                    Lưu
                                </ButtonStyled>
                            </div>
                        </div>
                    )}
                </ModalBody>
            </ModalContent>
        </ModalStyled>
    )
}
