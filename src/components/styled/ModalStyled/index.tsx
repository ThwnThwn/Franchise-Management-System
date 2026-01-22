"use client"
import React from "react"
import {
    Modal,
    ModalProps,
    ModalHeader,
    ModalHeaderProps,
    ModalContent,
    ModalContentProps,
    ModalBody,
    ModalBodyProps,
    ModalFooter,
    ModalFooterProps,
    useDisclosure
} from "@heroui/react"
import { cn } from "@heroui/react"

export function ModalStyled(props: ModalProps) {
    return (
        <Modal
            placement="top"
            color="secondary"
            isDismissable={false}
            size="lg"
            {...props}
            className={cn("min-w-sm", props.className)}
        />
    )
}

export function ModalHeaderStyled(props: ModalHeaderProps) {
    return <ModalHeader {...props} className={cn("text-lg font-semibold", props.className)} />
}

export function ModalContentStyled(props: ModalContentProps) {
    return <ModalContent {...props} className={cn("w-full", props.className)} />
}

export function ModalBodyStyled(props: ModalBodyProps) {
    return <ModalBody {...props} className={cn("space-y-4", props.className)} />
}

export function ModalFooterStyled(props: ModalFooterProps) {
    return (
        <ModalFooter {...props} className={cn("flex items-center justify-end", props.className)} />
    )
}

export const useModalDisclosure = useDisclosure
