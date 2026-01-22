"use client"

import { ButtonStyled, ModalHeaderStyled, ModalStyled } from "@/components"
import { STRINGS } from "@/constants/strings"
import { ModalBody, ModalContent, ModalFooter } from "@heroui/react"
import React from "react"

interface AlertModalProps {
    header: string
    body: string
    isOpen: boolean
    isDismissable?: boolean
    onOpenChange: () => void
    onClose: () => void
    onConfirm: () => void
    btnColor?: "success" | "warning" | "danger" | "default" | "primary" | "secondary"
}

export function AlertModal({
    header,
    body,
    isOpen,
    isDismissable = true,
    onOpenChange,
    onClose,
    onConfirm,
    btnColor = "primary"
}: AlertModalProps) {
    return (
        <ModalStyled
            isOpen={isOpen}
            onOpenChange={onOpenChange}
            isDismissable={isDismissable}
            placement="center"
        >
            <ModalContent className="w-fit max-w-screen">
                <ModalHeaderStyled>{header}</ModalHeaderStyled>
                <ModalBody>{body}</ModalBody>
                <ModalFooter>
                    <ButtonStyled
                        color={btnColor}
                        variant="ghost"
                        onPress={() => {
                            onConfirm()
                            onClose()
                        }}
                    >
                        {STRINGS.COMMON.CONFIRM}
                    </ButtonStyled>
                    <ButtonStyled onPress={onClose}>{STRINGS.COMMON.CANCEL}</ButtonStyled>
                </ModalFooter>
            </ModalContent>
        </ModalStyled>
    )
}
