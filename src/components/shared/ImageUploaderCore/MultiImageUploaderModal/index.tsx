/* eslint-disable @next/next/no-img-element */
"use client"

import { useCallback, useEffect, useState } from "react"
import { ModalStyled, ButtonStyled, ImageCropper, FileDropZone } from "@/components/"
import { ModalContent, ModalBody, ModalHeader, Chip, cn } from "@heroui/react"
import { getCroppedImage } from "@/utils/helpers/image"
import { Trash, Crop, AlertCircle } from "lucide-react"

type FileWithPreview = {
    id: string
    file?: File
    preview: string
    isExisting?: boolean
}

type MultiImageUploaderModalProps = {
    isOpen: boolean
    onOpenChange: () => void
    onClose: () => void
    uploadFn: (formData: FormData) => Promise<any>
    isUploadPending: boolean
    formDataKey?: string
    initialImages?: { id: string; url: string }[]

    // Config Crop
    // aspect?: number
    cropShape?: "rect" | "round"
    cropSize?: { width: number; height: number }

    // Config Logic
    label?: string
    maxFiles?: number // Mặc định 10
    minFiles?: number // Mặc định 1
}

export function MultiImageUploaderModal({
    isOpen,
    onOpenChange,
    onClose,
    uploadFn,
    isUploadPending,
    formDataKey = "images",
    initialImages,
    // aspect = 400 / 300,
    cropShape = "rect",
    cropSize = { width: 400, height: 300 },
    label,
    maxFiles = 10,
    minFiles = 1
}: MultiImageUploaderModalProps) {
    const aspect = cropSize.width / cropSize.height
    // Validate props
    const validMax = Math.max(1, maxFiles)
    const validMin = Math.max(1, Math.min(minFiles, validMax))

    const [selectedFiles, setSelectedFiles] = useState<FileWithPreview[]>([])
    const [deletedImageIds, setDeletedImageIds] = useState<string[]>([])

    const [fileToCropId, setFileToCropId] = useState<string | null>(null)
    const [croppedAreaPixels, setCroppedAreaPixels] = useState<any>(null)

    // Reset state & Cleanup
    const handleClose = () => {
        selectedFiles.forEach((f) => URL.revokeObjectURL(f.preview))
        setSelectedFiles([])
        setFileToCropId(null)
        onClose()
    }

    const handleOpenChange = () => {
        if (isOpen) handleClose()
        else onOpenChange()
    }

    useEffect(() => {
        return () => {
            selectedFiles.forEach((f) => URL.revokeObjectURL(f.preview))
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [])

    // Khi Modal mở lên, nếu có ảnh cũ thì nạp vào state
    useEffect(() => {
        if (isOpen && initialImages) {
            const existing = initialImages.map((img) => ({
                id: img.id,
                preview: img.url,
                isExisting: true
            }))
            setSelectedFiles(existing)
            setDeletedImageIds([]) // Reset rác mỗi lần mở
        }
    }, [isOpen, initialImages])

    const onDropFiles = useCallback(
        (files: File[]) => {
            const newFiles = files.map((file) => ({
                id: Math.random().toString(36).substring(7) + Date.now(),
                file: file,
                preview: URL.createObjectURL(file)
            }))

            setSelectedFiles((prev) => {
                const combined = [...prev, ...newFiles]
                return combined.slice(0, validMax) // Cắt bớt nếu vượt quá Max
            })
        },
        [validMax]
    )

    const removeFile = (id: string) => {
        setSelectedFiles((prev) => {
            const fileToRemove = prev.find((f) => f.id === id)

            // Nếu là ảnh cũ thì bỏ vào sọt rác
            if (fileToRemove?.isExisting) {
                setDeletedImageIds((prevIds) => [...prevIds, id])
            } else if (fileToRemove?.preview) {
                URL.revokeObjectURL(fileToRemove.preview)
            }

            return prev.filter((f) => f.id !== id)
        })
    }

    const handleCropSave = async () => {
        const currentFileObj = selectedFiles.find((f) => f.id === fileToCropId)
        if (!currentFileObj || !currentFileObj.file || !croppedAreaPixels) return

        try {
            const croppedBlob = await getCroppedImage(currentFileObj.preview, croppedAreaPixels)
            const newFile = new File([croppedBlob], currentFileObj.file.name, {
                type: currentFileObj.file.type,
                lastModified: Date.now()
            })
            const newPreview = URL.createObjectURL(newFile)

            setSelectedFiles((prev) =>
                prev.map((f) => {
                    if (f.id === fileToCropId) {
                        URL.revokeObjectURL(f.preview)
                        return { ...f, file: newFile, preview: newPreview }
                    }
                    return f
                })
            )

            setFileToCropId(null)
            setCroppedAreaPixels(null)
        } catch (e) {
            console.error(e)
        }
    }

    const handleUploadAll = async () => {
        const formData = new FormData()

        // 1. Gửi ảnh mới (chỉ những thằng không có cờ isExisting)
        selectedFiles.forEach((fileObj) => {
            if (!fileObj.isExisting && fileObj.file) {
                formData.append(formDataKey, fileObj.file)
            }
        })

        // 2. Gửi danh sách ID cần xóa (Dưới dạng mảng hoặc chuỗi phân cách bởi dấu phẩy)
        if (deletedImageIds.length > 0) {
            deletedImageIds.forEach((id) => {
                formData.append("deletedImageIds", id)
            })
        }

        await uploadFn(formData)
        handleClose()
    }

    const fileToCrop = selectedFiles.find((f) => f.id === fileToCropId)
    const isMinFilesReached = selectedFiles.length >= validMin

    return (
        <ModalStyled
            isOpen={isOpen}
            onOpenChange={() => {
                if (!isUploadPending) handleOpenChange()
            }}
            size={fileToCropId ? "2xl" : "4xl"}
        >
            <ModalContent className="w-full">
                <ModalHeader className="px-6 py-4">
                    <div className="flex items-center gap-3">
                        <span className="text-xl font-bold text-gray-800">
                            {fileToCropId ? "Chỉnh sửa ảnh" : label || "Tải ảnh lên"}
                        </span>
                        {!fileToCropId && (
                            <Chip
                                size="sm"
                                variant="flat"
                                color={isMinFilesReached ? "success" : "warning"}
                            >
                                {selectedFiles.length}/{validMax} ảnh
                            </Chip>
                        )}
                    </div>
                </ModalHeader>

                <ModalBody className="p-6">
                    {/* VIEW 1: CROP EDITOR */}
                    {fileToCropId && fileToCrop ? (
                        <div className="flex flex-col items-center gap-6 animate-in fade-in zoom-in duration-300 h-full justify-center">
                            <div className="flex flex-col items-center gap-6 animate-in fade-in zoom-in duration-300">
                                <ImageCropper
                                    imgSrc={fileToCrop.preview}
                                    onCropComplete={setCroppedAreaPixels}
                                    aspect={aspect}
                                    cropShape={cropShape}
                                    cropSize={cropSize}
                                    className="rounded-lg shadow-lg border border-gray-200"
                                />

                                <div className="flex gap-3 w-full justify-center">
                                    <ButtonStyled
                                        onPress={() => setFileToCropId(null)}
                                        isDisabled={isUploadPending}
                                    >
                                        Huỷ
                                    </ButtonStyled>

                                    <ButtonStyled
                                        color="primary"
                                        onPress={handleCropSave}
                                        isLoading={isUploadPending}
                                        className="px-8 font-semibold"
                                    >
                                        Cắt
                                    </ButtonStyled>
                                </div>
                            </div>
                        </div>
                    ) : (
                        // VIEW 2: GRID & DROPZONE
                        <div className="flex flex-col md:flex-row h-full gap-6">
                            {selectedFiles.length < validMax && (
                                <div
                                    className={cn(
                                        "flex flex-col gap-2 w-full md:w-xl",
                                        selectedFiles.length <= 0 && "md:w-full md:h-full"
                                    )}
                                >
                                    {/* DropZone */}
                                    <FileDropZone
                                        onFilesSelect={onDropFiles}
                                        multiple={true}
                                        maxFiles={validMax - selectedFiles.length} // Chỉ cho chọn số lượng còn thiếu
                                        className="md:h-full"
                                        disabled={isUploadPending}
                                    />

                                    {/* Warning Min Files */}
                                    {!isMinFilesReached && selectedFiles.length > 0 && (
                                        <div className="flex items-center gap-2 text-warning text-sm bg-warning/10 p-2 rounded-lg">
                                            <AlertCircle size={16} />
                                            <span>
                                                Vui lòng chọn tối thiểu {validMin} ảnh để tiếp tục.
                                            </span>
                                        </div>
                                    )}
                                </div>
                            )}

                            {/* Grid Ảnh */}
                            {selectedFiles.length > 0 && (
                                <div className="grid grid-cols-2 sm:grid-cols-3 gap-4 p-1 pr-2">
                                    {selectedFiles.map((item) => (
                                        <div
                                            key={item.id}
                                            style={{
                                                aspectRatio: `${cropSize.width} / ${cropSize.height}`
                                            }}
                                            className={`relative group rounded-xl overflow-hidden border border-gray-200 bg-white shadow-sm hover:shadow-md transition-all`}
                                        >
                                            <img
                                                src={item.preview}
                                                alt="Preview"
                                                className="object-contain w-full h-full" // Thêm w-full h-full để nó lấp đầy khung cha
                                                width={cropSize.width}
                                                height={cropSize.height}
                                            />

                                            <div className="absolute top-2 left-2">
                                                {item.isExisting ? (
                                                    <Chip
                                                        size="sm"
                                                        color="default"
                                                        variant="flat"
                                                        className="bg-white/80"
                                                    >
                                                        Cũ
                                                    </Chip>
                                                ) : (
                                                    <Chip
                                                        size="sm"
                                                        color="primary"
                                                        variant="flat"
                                                        className="bg-primary/80 text-white"
                                                    >
                                                        Mới
                                                    </Chip>
                                                )}
                                            </div>

                                            <div className="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center gap-3 backdrop-blur-[1px]">
                                                {!item.isExisting && (
                                                    <button
                                                        onClick={() => setFileToCropId(item.id)}
                                                        disabled={isUploadPending}
                                                        className="p-2 bg-white rounded-full text-gray-700 hover:text-primary hover:scale-110 transition-all shadow-lg"
                                                        title="Cắt ảnh"
                                                    >
                                                        <Crop size={16} />
                                                    </button>
                                                )}
                                                <button
                                                    onClick={() => removeFile(item.id)}
                                                    disabled={isUploadPending}
                                                    className="p-2 bg-white rounded-full text-gray-700 hover:text-danger hover:scale-110 transition-all shadow-lg"
                                                    title="Xóa"
                                                >
                                                    <Trash size={16} />
                                                </button>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>
                    )}
                </ModalBody>

                {/* FOOTER */}
                {!fileToCropId && selectedFiles.length > 0 && (
                    <div className="px-6 py-4 border-t border-gray-100 bg-gray-50 flex justify-end gap-3 rounded-b-xl">
                        <ButtonStyled
                            onPress={handleClose}
                            isDisabled={isUploadPending}
                            variant="light"
                            color="danger"
                        >
                            Hủy bỏ
                        </ButtonStyled>
                        <ButtonStyled
                            onPress={handleUploadAll}
                            isLoading={isUploadPending}
                            color="primary"
                            className="px-6 font-semibold shadow-md"
                            isDisabled={!isMinFilesReached} // Disable nếu chưa đủ min
                        >
                            {isUploadPending
                                ? "Đang tải lên..."
                                : `Tải lên ${selectedFiles.length} ảnh`}
                        </ButtonStyled>
                    </div>
                )}
            </ModalContent>
        </ModalStyled>
    )
}
