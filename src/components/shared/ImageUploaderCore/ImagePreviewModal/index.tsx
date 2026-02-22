"use client"

import { ModalStyled } from "@/components/styled"
import { ModalBody, ModalContent, ModalProps } from "@heroui/react"
import Image from "next/image"
import React from "react"

export interface ImagePreviewModalProps extends Omit<ModalProps, "children"> {
    imgUrl: string
    width: number
    height: number
}

export const ImagePreviewModal = ({
    isOpen,
    onOpenChange,
    imgUrl,
    width,
    height
}: ImagePreviewModalProps) => {
    return (
        <ModalStyled
            isOpen={isOpen}
            onOpenChange={onOpenChange}
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
                        src={imgUrl}
                        alt="Full Preview"
                        className="object-contain rounded-md"
                        width={width}
                        height={height}
                    />
                </ModalBody>
            </ModalContent>
        </ModalStyled>
    )
}
