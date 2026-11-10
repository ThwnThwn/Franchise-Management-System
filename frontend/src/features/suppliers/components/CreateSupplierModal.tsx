"use client"

import React, { useState } from "react"
import {
    ModalContent,
    ModalHeader,
    ModalBody,
    ModalFooter,
    Button
} from "@heroui/react"
import { ModalStyled, InputStyled, ButtonStyled } from "@/components"
import { Plus, Building2 } from "lucide-react"
import { CreateSupplierRequest } from "../models/request"

interface CreateSupplierModalProps {
    isOpen: boolean
    onClose: () => void
    onSubmit: (req: CreateSupplierRequest) => void
    isLoading?: boolean
}

const INITIAL_FORM: CreateSupplierRequest = {
    code: "",
    name: "",
    contactPerson: "",
    phone: "",
    email: "",
    address: "",
    taxCode: ""
}

export function CreateSupplierModal({
    isOpen,
    onClose,
    onSubmit,
    isLoading = false
}: CreateSupplierModalProps) {
    const [form, setForm] = useState<CreateSupplierRequest>(INITIAL_FORM)

    const handleChange = (field: keyof CreateSupplierRequest, value: string) => {
        setForm((prev) => ({ ...prev, [field]: value }))
    }

    const handleSubmit = () => {
        if (!form.name || !form.code) return
        onSubmit(form)
    }

    React.useEffect(() => {
        if (isOpen) {
            setForm(INITIAL_FORM)
        }
    }, [isOpen])

    return (
        <ModalStyled isOpen={isOpen} onClose={onClose} size="lg" scrollBehavior="outside">
            <ModalContent>
                <ModalHeader className="flex items-center gap-2">
                    <Plus className="w-5 h-5 text-primary" />
                    <span>Thêm nhà cung cấp mới</span>
                </ModalHeader>

                <ModalBody>
                    <div className="space-y-4">
                        <div className="flex items-center gap-3 bg-blue-50 p-4 rounded-xl border border-blue-100">
                            <Building2 className="w-6 h-6 text-blue-500" />
                            <p className="text-sm text-blue-700">
                                Nhập thông tin để tạo nhà cung cấp mới vào hệ thống.
                            </p>
                        </div>

                        <div className="grid grid-cols-2 gap-3">
                            <InputStyled
                                label="Mã nhà cung cấp"
                                placeholder="NCC001"
                                isRequired
                                value={form.code}
                                onChange={(e) => handleChange("code", e.target.value)}
                            />
                            <InputStyled
                                label="Tên nhà cung cấp"
                                placeholder="Công ty ABC"
                                isRequired
                                value={form.name}
                                onChange={(e) => handleChange("name", e.target.value)}
                            />
                        </div>

                        <div className="grid grid-cols-2 gap-3">
                            <InputStyled
                                label="Người liên hệ"
                                placeholder="Nguyễn Văn A"
                                value={form.contactPerson}
                                onChange={(e) => handleChange("contactPerson", e.target.value)}
                            />
                            <InputStyled
                                label="Số điện thoại"
                                placeholder="0901234567"
                                value={form.phone}
                                onChange={(e) => handleChange("phone", e.target.value)}
                            />
                        </div>

                        <InputStyled
                            label="Email"
                            placeholder="ncc@example.com"
                            type="email"
                            value={form.email}
                            onChange={(e) => handleChange("email", e.target.value)}
                        />

                        <InputStyled
                            label="Địa chỉ"
                            placeholder="Số 123, Đường X, Quận Y..."
                            value={form.address}
                            onChange={(e) => handleChange("address", e.target.value)}
                        />

                        <InputStyled
                            label="Mã số thuế"
                            placeholder="0123456789"
                            value={form.taxCode}
                            onChange={(e) => handleChange("taxCode", e.target.value)}
                        />
                    </div>
                </ModalBody>

                <ModalFooter>
                    <Button variant="light" color="danger" onPress={onClose} isDisabled={isLoading}>
                        Hủy
                    </Button>
                    <ButtonStyled
                        color="primary"
                        onPress={handleSubmit}
                        isLoading={isLoading}
                        isDisabled={!form.name || !form.code}
                    >
                        Tạo nhà cung cấp
                    </ButtonStyled>
                </ModalFooter>
            </ModalContent>
        </ModalStyled>
    )
}
