"use client"

import { Card, CardBody } from "@heroui/react"
import { Image as ImageIcon, X, Loader2 } from "lucide-react"
import { useFormikContext } from "formik"
import { useState } from "react"

import { FileDropZone } from "@/components"
import { useFormikHelpers } from "@/hooks"
import { SubmitApplicationRequest } from "@/features/franchise/models/request"
import { resizeImage } from "@/utils/helpers/image"

interface FileWithPreview extends File {
    preview?: string
}

const ImagePreview = ({ file, onRemove }: { file: FileWithPreview; onRemove: () => void }) => {
    return (
        <div className="relative group rounded-xl overflow-hidden border border-default-200 aspect-4/3 shadow-sm hover:shadow-md transition-all">
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img src={file.preview} alt={file.name} className="object-cover w-full h-full" />
            <div className="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center backdrop-blur-[1px]">
                <button
                    type="button"
                    onClick={onRemove}
                    className="p-2 bg-danger text-white rounded-full hover:scale-110 transition-transform shadow-lg"
                >
                    <X size={18} />
                </button>
            </div>
            <div className="absolute bottom-0 left-0 right-0 bg-black/60 text-white text-[10px] truncate px-2 py-1">
                {file.name}
            </div>
        </div>
    )
}

export const AttachmentCard = () => {
    const formik = useFormikContext<SubmitApplicationRequest>()
    const { isInvalid, getErrorMessage } = useFormikHelpers(formik)
    const { values, setFieldValue } = formik

    const [isProcessing, setIsProcessing] = useState(false)

    const handleProcessFiles = async (files: File[]) => {
        setIsProcessing(true)
        try {
            const processedFiles = await Promise.all(
                files.map(async (file) => {
                    const resizedBlob = await resizeImage(file)
                    // Chuyển Blob ngược lại thành File để gửi qua FormData
                    const resizedFile = new File([resizedBlob], file.name, {
                        type: "image/jpeg",
                        lastModified: Date.now()
                    })
                    return Object.assign(resizedFile, {
                        preview: URL.createObjectURL(resizedFile)
                    })
                })
            )
            return processedFiles
        } finally {
            setIsProcessing(false)
        }
    }

    const handleRemoveFile = (
        fieldName: "storeFront" | "landCerts",
        file: FileWithPreview,
        idx?: number
    ) => {
        if (file.preview) URL.revokeObjectURL(file.preview)

        if (fieldName === "storeFront") {
            setFieldValue("storeFront", undefined)
        } else {
            const filtered = values.landCerts?.filter((_, index) => index !== idx)
            setFieldValue("landCerts", filtered)
        }
    }

    return (
        <Card shadow="sm" className="border border-default-200 h-fit">
            <CardBody className="p-6 bg-default-50/30">
                <h3 className="text-sm font-black uppercase text-warning-600 tracking-wider mb-4 flex items-center gap-2">
                    {isProcessing ? (
                        <Loader2 className="animate-spin" size={18} />
                    ) : (
                        <ImageIcon size={18} />
                    )}
                    3. Hình ảnh đính kèm{" "}
                    {isProcessing && (
                        <span className="text-xs lowercase font-normal">(Đang tối ưu ảnh...)</span>
                    )}
                </h3>

                <div className="space-y-8">
                    {/* ===== 1. ẢNH MẶT TIỀN ===== */}
                    <div>
                        <label className="block text-sm font-bold text-default-700 mb-2">
                            Ảnh mặt tiền hiện trạng (1 ảnh) <span className="text-danger">*</span>
                        </label>
                        {!values.storeFront ? (
                            <FileDropZone
                                multiple={false}
                                disabled={isProcessing}
                                onFileSelect={async (file) => {
                                    const processed = await handleProcessFiles([file])
                                    setFieldValue("storeFront", processed[0])
                                }}
                            />
                        ) : (
                            <div className="w-1/2 sm:w-1/3">
                                <ImagePreview
                                    file={values.storeFront as FileWithPreview}
                                    onRemove={() =>
                                        handleRemoveFile(
                                            "storeFront",
                                            values.storeFront as FileWithPreview
                                        )
                                    }
                                />
                            </div>
                        )}
                        {isInvalid("storeFront") && (
                            <p className="text-tiny text-danger mt-2">
                                {getErrorMessage("storeFront")}
                            </p>
                        )}
                    </div>

                    <div className="w-full h-px bg-default-200" />

                    {/* ===== 2. ẢNH SỔ ĐỎ ===== */}
                    <div>
                        <label className="block text-sm font-bold text-default-700 mb-2">
                            Sổ đỏ / Giấy tờ đất (Nhiều ảnh) <span className="text-danger">*</span>
                        </label>
                        <FileDropZone
                            multiple={true}
                            maxFiles={10}
                            disabled={isProcessing}
                            className="mb-4"
                            onFilesSelect={async (files) => {
                                const processed = await handleProcessFiles(files)
                                setFieldValue(
                                    "landCerts",
                                    [...(values.landCerts || []), ...processed].slice(0, 10)
                                )
                            }}
                        />
                        {isInvalid("landCerts") && (
                            <p className="text-tiny text-danger mb-2">
                                {getErrorMessage("landCerts")}
                            </p>
                        )}

                        {values.landCerts && values.landCerts.length > 0 && (
                            <div className="grid grid-cols-2 sm:grid-cols-3 gap-4">
                                {values.landCerts.map((file, idx) => (
                                    <ImagePreview
                                        key={`${file.name}-${idx}`}
                                        file={file as FileWithPreview}
                                        onRemove={() =>
                                            handleRemoveFile(
                                                "landCerts",
                                                file as FileWithPreview,
                                                idx
                                            )
                                        }
                                    />
                                ))}
                            </div>
                        )}
                    </div>
                </div>
            </CardBody>
        </Card>
    )
}
