"use client"

import { useCallback } from "react"
import { useDropzone } from "react-dropzone"
import { cn } from "@heroui/react"
import { CloudUpload } from "lucide-react"

type FileDropZoneProps = {
    onFileSelect?: (file: File) => void
    onFilesSelect?: (files: File[]) => void
    className?: string
    multiple?: boolean
    maxFiles?: number
    disabled?: boolean
}

export function FileDropZone({
    onFileSelect,
    onFilesSelect,
    className,
    multiple = false,
    maxFiles = 1,
    disabled = false
}: FileDropZoneProps) {
    const onDrop = useCallback(
        (acceptedFiles: File[]) => {
            if (acceptedFiles && acceptedFiles.length > 0) {
                if (multiple && onFilesSelect) {
                    onFilesSelect(acceptedFiles)
                } else if (onFileSelect) {
                    onFileSelect(acceptedFiles[0])
                }
            }
        },
        [multiple, onFileSelect, onFilesSelect]
    )

    const { getRootProps, getInputProps, isDragActive, fileRejections } = useDropzone({
        onDrop,
        accept: {
            "image/jpeg": [],
            "image/png": [],
            "image/webp": []
        },
        maxFiles: multiple ? maxFiles || 0 : 1,
        multiple: multiple,
        disabled: disabled
    })

    // Thông báo lỗi nếu chọn quá số lượng
    const isFileTooMany =
        fileRejections.length > 0 && fileRejections[0].errors[0].code === "too-many-files"

    return (
        <div
            {...getRootProps()}
            className={cn(
                "border-2 border-dashed rounded-xl p-6 flex flex-col items-center justify-center cursor-pointer transition-colors relative",
                !className?.includes("h-") && "h-75",
                isDragActive && !disabled
                    ? "border-primary bg-primary/10"
                    : "border-gray-300 hover:border-gray-400 hover:bg-gray-50",
                disabled && "opacity-50 cursor-not-allowed bg-gray-100 hover:border-gray-300",
                isFileTooMany && "border-danger bg-danger/5",
                className
            )}
        >
            <input {...getInputProps()} />

            <div
                className={cn(
                    "mb-4 p-4 rounded-full",
                    isFileTooMany ? "bg-danger/10" : "bg-gray-100"
                )}
            >
                <CloudUpload
                    size={32}
                    className={cn(isFileTooMany ? "text-danger" : "text-gray-500")}
                />
            </div>

            {isDragActive ? (
                <p className="text-primary font-medium">Thả ảnh vào đây ngay...</p>
            ) : (
                <div className="text-center space-y-2">
                    <p
                        className={cn(
                            "text-sm font-semibold",
                            isFileTooMany ? "text-danger" : "text-gray-700"
                        )}
                    >
                        {isFileTooMany
                            ? `Bạn chỉ được chọn tối đa ${maxFiles} ảnh!`
                            : multiple
                              ? "Kéo thả nhiều ảnh hoặc nhấn để chọn"
                              : "Nhấn để tải lên hoặc kéo thả vào đây"}
                    </p>
                    <p className="text-xs text-gray-500 mb-0">PNG, JPG hoặc WebP</p>
                    <p className="text-xs text-gray-500">
                        {maxFiles > 1 ? `(Còn tối đa ${maxFiles} files)` : "(Max. 10MB)"}
                    </p>
                </div>
            )}
        </div>
    )
}
