"use client"

import React from "react"
import { ModalBody, ModalContent, ModalFooter } from "@heroui/react"
import { AlertTriangle, CheckCircle, Info, Trash2 } from "lucide-react"
import { ButtonStyled, ModalHeaderStyled, ModalStyled } from "@/components"
import { STRINGS } from "@/constants/strings"

type AlertVariant = "danger" | "success" | "warning" | "info"

interface AlertModalProps {
    isOpen: boolean
    onClose: () => void
    onConfirm: () => void
    header?: string
    body: React.ReactNode // Đổi thành Node để truyền component phức tạp nếu cần
    variant?: AlertVariant
    confirmText?: string
    cancelText?: string
    isLoading?: boolean // Thêm loading khi đang gọi API
}

export function AlertModal({
    isOpen,
    onClose,
    onConfirm,
    header,
    body,
    variant = "danger", // Mặc định là danger vì modal này hay dùng để xóa
    confirmText = STRINGS.COMMON.CONFIRM,
    cancelText = STRINGS.COMMON.CANCEL,
    isLoading = false
}: AlertModalProps) {
    // Config giao diện dựa trên variant
    const getVariantConfig = () => {
        switch (variant) {
            case "danger":
                return {
                    color: "danger" as const,
                    icon: <Trash2 className="w-6 h-6 text-danger" />,
                    defaultHeader: "Xác nhận xóa"
                }
            case "success":
                return {
                    color: "success" as const,
                    icon: <CheckCircle className="w-6 h-6 text-success" />,
                    defaultHeader: "Thành công"
                }
            case "warning":
                return {
                    color: "warning" as const,
                    icon: <AlertTriangle className="w-6 h-6 text-warning" />,
                    defaultHeader: "Cảnh báo"
                }
            default:
                return {
                    color: "primary" as const,
                    icon: <Info className="w-6 h-6 text-primary" />,
                    defaultHeader: "Thông báo"
                }
        }
    }

    const config = getVariantConfig()

    return (
        <ModalStyled
            isOpen={isOpen}
            onOpenChange={onClose}
            isDismissable={!isLoading} // Không cho tắt khi đang loading
            hideCloseButton={isLoading}
            placement="center"
            backdrop="blur"
        >
            <ModalContent className="max-w-md">
                <ModalHeaderStyled className="flex flex-col gap-1 items-center justify-center pt-6">
                    <div className={`p-3 rounded-full bg-${config.color}-50 mb-2`}>
                        {config.icon}
                    </div>
                    <span className="text-xl font-bold">{header || config.defaultHeader}</span>
                </ModalHeaderStyled>

                <ModalBody className="text-center text-slate-500 pb-2">{body}</ModalBody>

                <ModalFooter className="flex justify-center gap-3 pb-6">
                    {!isLoading && (
                        <ButtonStyled
                            onPress={onClose}
                            variant="light"
                            color="default"
                            isDisabled={isLoading}
                            className="font-medium"
                        >
                            {cancelText}
                        </ButtonStyled>
                    )}

                    <ButtonStyled
                        color={config.color}
                        onPress={onConfirm}
                        isLoading={isLoading}
                        className="font-semibold shadow-md"
                    >
                        {confirmText}
                    </ButtonStyled>
                </ModalFooter>
            </ModalContent>
        </ModalStyled>
    )
}
