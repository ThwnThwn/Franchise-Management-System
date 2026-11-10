"use client"

import { useEffect } from "react"
import { DatePickerStyled, InputStyled, ModalContentStyled, ModalStyled } from "@/components/styled"
import { addToast, ModalBody, ModalHeader, Spinner } from "@heroui/react"
import { useGetUserById } from "@/features/users/hooks"
import { BackendError } from "@/models/common/respone"
import { STRINGS } from "@/constants/strings"
import { useDay, useUserHelper } from "@/hooks"
import { EnumPicker } from "@/components"
import { GenderLabels } from "@/constants/labels"
import { DEFAULT_DATE_FORMAT } from "@/constants/constants"

export function ViewUserModal({
    isOpen,
    onOpenChange,
    userId
}: {
    isOpen: boolean
    onOpenChange: (isOpen: boolean) => void
    userId: string
}) {
    const { toFullName } = useUserHelper()
    const { data, isLoading, error } = useGetUserById({ id: userId, enabled: isOpen })
    const { toDate } = useDay({ defaultFormat: DEFAULT_DATE_FORMAT })

    useEffect(() => {
        if (error) {
            const beErr = error as BackendError
            addToast({
                title: STRINGS.COMMON.ERROR,
                description: beErr.detail,
                color: "danger"
            })
        }
    }, [error])

    return (
        <ModalStyled isOpen={isOpen} onOpenChange={onOpenChange} size="xl" isDismissable>
            <ModalContentStyled className="w-full">
                <ModalHeader className="flex flex-col items-center font-semibold text-3xl">
                    {"Thông tin người dùng"}
                </ModalHeader>
                <ModalBody>
                    {isLoading ? (
                        <Spinner />
                    ) : (
                        <div className="flex flex-col gap-2">
                            <div className="flex gap-2">
                                <InputStyled label="Username" value={data?.username} readOnly />
                                <InputStyled label="Email" value={data?.email} readOnly />
                            </div>
                            <InputStyled
                                label="Họ và tên"
                                value={toFullName({
                                    firstName: data?.firstName,
                                    lastName: data?.lastName
                                })}
                                readOnly
                            />
                            <div className="flex gap-2">
                                <InputStyled label="Số điện thoại" value={data?.phone} readOnly />
                                <InputStyled label="Địa chỉ" value={data?.address} readOnly />
                            </div>
                            <div className="flex gap-2">
                                <EnumPicker
                                    label="Giới tính"
                                    labels={GenderLabels}
                                    value={data?.gender}
                                    isReadOnly
                                />
                                <DatePickerStyled
                                    label="Ngày sinh"
                                    value={toDate(data?.dateOfBirth)}
                                />
                            </div>
                        </div>
                    )}
                </ModalBody>
            </ModalContentStyled>
        </ModalStyled>
    )
}
