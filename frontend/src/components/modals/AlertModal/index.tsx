"use client"

import React, { useState } from "react"
import { ModalBody, ModalContent, ModalFooter } from "@heroui/react"
import { AlertTriangle, CheckCircle, Info, Trash2 } from "lucide-react"
import { ButtonStyled, ModalHeaderStyled, ModalStyled, TextareaStyled } from "@/components"
import { STRINGS } from "@/constants/strings"

type AlertVariant = "danger" | "success" | "warning" | "info"

interface AlertModalProps {
    isOpen: boolean
    onClose: () => void
    onConfirm: (inputValue?: string) => void
    header?: string
    body: React.ReactNode // Đổi thành Node để truyền component phức tạp nếu cần
    variant?: AlertVariant
    confirmText?: string
    cancelText?: string
    isLoading?: boolean // Thêm loading khi đang gọi API

    withInput?: boolean
    inputPlaceholder?: string
    isInputRequired?: boolean
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
    isLoading = false,

    withInput = false,
    inputPlaceholder = "Vui lòng nhập lý do...",
    isInputRequired = true
}: AlertModalProps) {
    const [inputValue, setInputValue] = useState("")

    const [prevIsOpen, setPrevIsOpen] = useState(isOpen)
    // Nếu trạng thái isOpen thay đổi so với lần render trước
    if (isOpen !== prevIsOpen) {
        setPrevIsOpen(isOpen) // Cập nhật lại trạng thái theo dõi
        if (isOpen) {
            setInputValue("") // Xóa text ngay lập tức khi modal vừa mở lên
        }
    }

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

                <ModalBody className="text-center text-slate-500 pb-2">
                    {body}
                    {withInput && (
                        <div className="mt-4 text-left animate-appearance-in">
                            <TextareaStyled
                                autoFocus
                                variant="bordered"
                                color={config.color}
                                placeholder={inputPlaceholder}
                                value={inputValue}
                                onValueChange={setInputValue}
                                minRows={2}
                                isRequired={isInputRequired}
                                classNames={{ input: "text-sm" }}
                            />
                        </div>
                    )}
                </ModalBody>

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
                        onPress={() => onConfirm(inputValue)}
                        isLoading={isLoading}
                        className="font-semibold shadow-md"
                        isDisabled={withInput && isInputRequired && inputValue.trim().length === 0}
                    >
                        {confirmText}
                    </ButtonStyled>
                </ModalFooter>
            </ModalContent>
        </ModalStyled>
    )
}
