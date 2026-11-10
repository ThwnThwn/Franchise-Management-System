/* eslint-disable @next/next/no-img-element */
"use client"

import { useCallback, useState } from "react"
import { ModalStyled, ButtonStyled, ImageCropper, FileDropZone } from "@/components/"
import { ModalContent, ModalBody, ModalHeader } from "@heroui/react"
import { getCroppedImage } from "@/utils/helpers/image"

type ImageUploaderModalProps = {
    label?: string
    saveLabel?: string

    isOpen: boolean
    onOpenChange: () => void
    onClose: () => void

    imgSrc: string | null
    setImgSrc: (src: string | null) => void
    // setDisplayImg?: (src: string | undefined) => void
    uploadFn: (formData: FormData) => Promise<any>
    isUploadPending: boolean
    formDataKey?: string

    // aspect?: number
    requireCrop?: boolean
    cropShape?: "rect" | "round"
    cropSize?: { width: number; height: number } // Đổi thành optional luôn cho tiện
}

export function ImageUploaderModal({
    label,
    saveLabel = "Lưu",
    //
    isOpen,
    onOpenChange,
    onClose,
    //
    imgSrc,
    setImgSrc,
    uploadFn,
    // setDisplayImg,
    isUploadPending,
    formDataKey = "image",
    //
    // aspect = 1,
    requireCrop = true, // Default vẫn là true để không ảnh hưởng code cũ
    cropShape = "rect",
    cropSize = { width: 400, height: 300 }
}: ImageUploaderModalProps) {
    const aspect = cropSize.width / cropSize.height

    const [selectedFile, setSelectedFile] = useState<File | null>(null)
    const [croppedAreaPixels, setCroppedAreaPixels] = useState<any>(null)

    const resetState = useCallback(() => {
        setCroppedAreaPixels(null)
        setSelectedFile(null)
        setImgSrc(null)
    }, [setImgSrc])

    const handleOpenChange = () => {
        if (isOpen) {
            resetState()
            onClose()
        } else {
            onOpenChange()
        }
    }

    // Xử lý khi Drop file thành công
    const onDropFile = useCallback(
        (file: File) => {
            setSelectedFile(file) // Giữ lại file gốc chuẩn bị up thẳng
            const reader = new FileReader()
            reader.addEventListener("load", () => {
                setImgSrc(reader.result as string)
            })
            reader.readAsDataURL(file)
        },
        [setImgSrc]
    )

    const handleUpload = useCallback(async () => {
        if (!imgSrc) return

        const formData = new FormData()

        if (requireCrop) {
            // NẾU CẦN CROP: Lấy Blob từ Cropper
            if (!croppedAreaPixels) return
            const blob = await getCroppedImage(imgSrc, croppedAreaPixels)
            formData.append(formDataKey, blob, "image.jpg")
        } else {
            // NẾU KHÔNG CROP: Lấy thẳng File gốc đẩy lên
            if (!selectedFile) return
            formData.append(formDataKey, selectedFile)
        }

        try {
            // Gọi hàm upload (việc xử lý logic bên trong đã có cha lo)
            await uploadFn(formData)

            // Hoàn tất: Reset và Đóng modal ngay lập tức
            resetState()
            onClose()
        } catch {
            /* empty */
        }
    }, [
        imgSrc,
        requireCrop,
        croppedAreaPixels,
        formDataKey,
        selectedFile,
        uploadFn,
        resetState,
        onClose
    ])

    // Nút Hủy hoặc Chọn lại ảnh khác
    const handleCancel = () => {
        if (imgSrc) {
            resetState() // Quay lại màn hình DropZone
        } else {
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
                            {requireCrop ? (
                                <ImageCropper
                                    imgSrc={imgSrc}
                                    onCropComplete={setCroppedAreaPixels}
                                    aspect={aspect}
                                    cropShape={cropShape}
                                    cropSize={cropSize}
                                    className="rounded-lg shadow-lg border border-default-200"
                                />
                            ) : (
                                <div className="w-full max-h-[60vh] flex items-center justify-center bg-default-100 rounded-lg overflow-hidden border border-default-200 p-2">
                                    <img
                                        src={imgSrc}
                                        alt="Preview"
                                        className="max-w-full max-h-full object-contain rounded-md shadow-sm"
                                    />
                                </div>
                            )}

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
                                    {saveLabel}
                                </ButtonStyled>
                            </div>
                        </div>
                    )}
                </ModalBody>
            </ModalContent>
        </ModalStyled>
    )
}
