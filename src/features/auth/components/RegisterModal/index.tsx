"use client"
import React from "react"
import { ModalBody, ModalContent, ModalHeader } from "@heroui/react"
import { ModalStyled } from "@/components"
import { useRegisterDiscloresureSingleton } from "@/hooks"
import { RegisterForm } from "./RegisterForm"
import { AUTH_TEXT } from "../../constants"

export function RegisterModal() {
    const { isOpen, onOpenChange, onClose } = useRegisterDiscloresureSingleton() //onclose

    return (
        <ModalStyled isOpen={isOpen} onOpenChange={onOpenChange}>
            <ModalContent className="w-full max-w-screen min-w-full sm:min-w-sm md:max-w-150 py-2 md:p-14">
                <ModalHeader className="flex justify-center items-center text-2xl front-bold mt">
                    {AUTH_TEXT.TITLES.REGISTER}
                </ModalHeader>
                <ModalBody>
                    <RegisterForm onSuccess={onClose} />
                </ModalBody>
            </ModalContent>
        </ModalStyled>
    )
}
