"use client"

import { CreateUserForm, CreateUserFormProps } from "./CreateUserForm"
import { ModalContentStyled, ModalStyled } from "@/components/styled"
import { ModalBody, ModalHeader } from "@heroui/react"

interface CreateUserModalProps extends CreateUserFormProps {
    isOpen: boolean
    onOpenChange: (isOpen: boolean) => void
}

export function CreateUserModal({
    isOpen,
    onOpenChange,
    createMutation,
    isCreateCustomer,
    createRoleSlug
}: CreateUserModalProps) {
    return (
        <ModalStyled isOpen={isOpen} onOpenChange={onOpenChange} isDismissable>
            <ModalContentStyled className="max-w-150 w-full">
                <ModalHeader className="flex flex-col items-center font-semibold text-3xl">
                    {"Tạo nhân viên mới"}
                </ModalHeader>
                <ModalBody>
                    <CreateUserForm
                        createMutation={createMutation}
                        isCreateCustomer={isCreateCustomer}
                        createRoleSlug={createRoleSlug}
                    />
                </ModalBody>
            </ModalContentStyled>
        </ModalStyled>
    )
}
