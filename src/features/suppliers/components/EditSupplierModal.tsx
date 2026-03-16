"use client"

import React, { useState, useEffect } from "react"
import {
    ModalContent,
    ModalHeader,
    ModalBody,
    ModalFooter,
    Button
} from "@heroui/react"
import { ModalStyled, InputStyled, ButtonStyled } from "@/components"
import { Edit3, Building2 } from "lucide-react"
import { UpdateSupplierRequest } from "../models/request"
import { SupplierItem } from "../models/response"

interface EditSupplierModalProps {
    isOpen: boolean
    onClose: () => void
    supplier: SupplierItem | null
    onSubmit: (id: string, req: Omit<UpdateSupplierRequest, "id">) => void
    isLoading?: boolean
}

export function EditSupplierModal({
    isOpen,
    onClose,
    supplier,
    onSubmit,
    isLoading = false
}: EditSupplierModalProps) {
    const [form, setForm] = useState<Omit<UpdateSupplierRequest, "id">>({
        code: "",
        name: "",
        contactPerson: "",
        phone: "",
        email: "",
        address: "",
        taxCode: "",
        isActive: true
    })

    useEffect(() => {
        if (isOpen && supplier) {
            setForm({
                code: supplier.code,
                name: supplier.name,
                contactPerson: supplier.contactPerson || "",
                phone: supplier.phone || "",
                email: supplier.email || "",
                address: supplier.address || "",
                taxCode: supplier.taxCode || "",
                isActive: supplier.isActive
            })
        }
    }, [isOpen, supplier])

    const handleChange = (field: keyof Omit<UpdateSupplierRequest, "id">, value: string | boolean) => {
        setForm((prev) => ({ ...prev, [field]: value }))
    }

    const handleSubmit = () => {
        if (!form.name || !form.code || !supplier) return
        onSubmit(supplier.id, form)
    }

    return (
        <ModalStyled isOpen={isOpen} onClose={onClose} size="lg" scrollBehavior="outside">
            <ModalContent>
                <ModalHeader className="flex items-center gap-2">
                    <Edit3 className="w-5 h-5 text-primary" />
                    <span>Cập nhật nhà cung cấp</span>
                </ModalHeader>

                <ModalBody>
                    <div className="space-y-4">
                        <div className="flex items-center gap-3 bg-blue-50 p-4 rounded-xl border border-blue-100">
                            <Building2 className="w-6 h-6 text-blue-500" />
                            <p className="text-sm text-blue-700">
                                Chỉnh sửa thông tin nhà cung cấp <strong>{supplier?.name}</strong>.
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
                                value={form.contactPerson as string}
                                onChange={(e) => handleChange("contactPerson", e.target.value)}
                            />
                            <InputStyled
                                label="Số điện thoại"
                                placeholder="0901234567"
                                value={form.phone as string}
                                onChange={(e) => handleChange("phone", e.target.value)}
                            />
                        </div>

                        <InputStyled
                            label="Email"
                            placeholder="ncc@example.com"
                            type="email"
                            value={form.email as string}
                            onChange={(e) => handleChange("email", e.target.value)}
                        />

                        <InputStyled
                            label="Địa chỉ"
                            placeholder="Số 123, Đường X, Quận Y..."
                            value={form.address as string}
                            onChange={(e) => handleChange("address", e.target.value)}
                        />

                        <InputStyled
                            label="Mã số thuế"
                            placeholder="0123456789"
                            value={form.taxCode as string}
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
                        Lưu thay đổi
                    </ButtonStyled>
                </ModalFooter>
            </ModalContent>
        </ModalStyled>
    )
}
