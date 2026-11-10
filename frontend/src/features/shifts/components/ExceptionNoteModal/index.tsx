"use client"

import React from "react"
import { ModalContent, ModalHeader, ModalBody, ModalFooter, Button } from "@heroui/react"
import { AlertCircle } from "lucide-react"
import { ModalStyled, ButtonStyled, TextareaStyled } from "@/components"

interface ExceptionNoteModalProps {
    isOpen: boolean
    onClose: () => void
    onSubmit: (note: string) => void
    isLoading: boolean
    title: string
}

export const ExceptionNoteModal = ({
    isOpen,
    onClose,
    onSubmit,
    isLoading,
    title
}: ExceptionNoteModalProps) => {
    const [note, setNote] = React.useState("")

    return (
        <ModalStyled isOpen={isOpen} onClose={onClose} size="md">
            <ModalContent>
                <ModalHeader className="flex gap-2 items-center">
                    <AlertCircle className="text-warning" size={20} />
                    <span>Xác nhận {title}</span>
                </ModalHeader>
                <ModalBody>
                    <p className="text-small text-default-500 mb-2">
                        Vị trí của bạn không hợp lệ hoặc lỗi GPS. Vui lòng nhập lý do để Quản lý
                        duyệt công.
                    </p>
                    <TextareaStyled
                        label="Lý do giải trình"
                        placeholder="VD: Máy em bị lỗi GPS, em đang đứng ở cửa..."
                        value={note}
                        onChange={(e) => setNote(e.target.value)}
                        variant="bordered"
                    />
                </ModalBody>
                <ModalFooter>
                    <Button variant="light" onPress={onClose}>
                        Hủy
                    </Button>
                    <ButtonStyled
                        color="warning"
                        isLoading={isLoading}
                        onPress={() => onSubmit(note)}
                    >
                        Gửi & Xác nhận
                    </ButtonStyled>
                </ModalFooter>
            </ModalContent>
        </ModalStyled>
    )
}
