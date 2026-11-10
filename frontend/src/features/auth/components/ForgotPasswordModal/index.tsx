"use client"
import React from "react"
import { ModalBody, ModalContent } from "@heroui/react"
import { ModalStyled } from "@/components/"
import { ForgotPasswordForm } from "./ForgotPasswordForm"
import { useForgotPasswordDiscloresureSingleton } from "@/hooks"

export function ForgotPasswordModal() {
    const { isOpen, onOpenChange, onClose } = useForgotPasswordDiscloresureSingleton()

    return (
        <ModalStyled isOpen={isOpen} onOpenChange={onOpenChange}>
            <ModalContent className="w-full max-w-screen min-w-full sm:min-w-sm md:max-w-150 py-2 md:p-14">
                <ModalBody>
                    <ForgotPasswordForm onSuccess={onClose} />
                </ModalBody>
            </ModalContent>
        </ModalStyled>
    )
}
