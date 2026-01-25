"use client"
import React from "react"
import { ModalBody, ModalContent } from "@heroui/react"
import { useLoginDiscloresureSingleton } from "@/hooks"
import { ModalStyled } from "@/components/"
import { LoginForm } from "./loginModal/loginForm"

export function LoginModal() {
    const { isOpen, onOpenChange, onClose } = useLoginDiscloresureSingleton()

    return (
        <ModalStyled isOpen={isOpen} onOpenChange={onOpenChange}>
            <ModalContent className="w-full max-w-screen min-w-full sm:min-w-sm md:max-w-150 py-2 md:p-14">
                <ModalBody>
                    <LoginForm onSuccess={onClose} />
                </ModalBody>
            </ModalContent>
        </ModalStyled>
    )
}
