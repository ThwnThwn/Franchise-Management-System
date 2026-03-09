"use client"

import React, { useState, useEffect } from "react"
import { Modal, ModalContent, ModalHeader, ModalBody, ModalFooter, Button, Input, Select, SelectItem, Card, CardBody, Divider } from "@heroui/react"
import { VoucherItemType, CreateVoucherItemReq } from "@/features/orders/models/voucher"
import { useGetAllProducts } from "@/features/products/hooks/useProduct"
import { Plus, Trash2, Save } from "lucide-react"

interface VoucherItemModalProps {
    isOpen: boolean
    onOpenChange: (isOpen: boolean) => void
    initialItems: CreateVoucherItemReq[]
    onSave: (items: CreateVoucherItemReq[]) => void
}

const voucherItemTypes = [
    { value: VoucherItemType.ITEM_GIFT, label: "Quà tặng (Item Gift)" },
    { value: VoucherItemType.ITEM_REQUIRED, label: "Yêu cầu (Item Required)" }
]

export const VoucherItemModal = ({ isOpen, onOpenChange, initialItems, onSave }: VoucherItemModalProps) => {
    const [items, setItems] = useState<CreateVoucherItemReq[]>([])

    // Fetch products for GIFT items
    const { data: productsData } = useGetAllProducts({
        params: {},
        pagination: { page: 1, pageSize: 100 },
        enabled: isOpen
    })

    useEffect(() => {
        if (isOpen) {
            setItems(initialItems || [])
        }
    }, [isOpen, initialItems])

    const handleAddItem = () => {
        setItems(prev => [
            ...prev,
            { variant_id: "", quantity: 1, type: VoucherItemType.ITEM_GIFT }
        ])
    }

    const handleRemoveItem = (index: number) => {
        setItems(prev => prev.filter((_, i) => i !== index))
    }

    const handleUpdateItem = (index: number, field: keyof CreateVoucherItemReq, value: any) => {
        setItems(prev => {
            const newItems = [...prev]
            newItems[index] = { ...newItems[index], [field]: value }
            return newItems
        })
    }

    const handleSave = () => {
        // Filter out items without variant_id
        const validItems = items.filter(item => !!item.variant_id)
        onSave(validItems)
        onOpenChange(false)
    }

    // Flatten products into selection list of variants
    const variantOptions = productsData?.items?.flatMap(p =>
        p.variants.map(v => ({
            id: v.id,
            label: `${p.name} - ${v.sku} ${v.attributes ? `(${v.attributes})` : ""}`
        }))
    ) || []

    return (
        <Modal isOpen={isOpen} onOpenChange={onOpenChange} size="2xl" scrollBehavior="inside">
            <ModalContent>
                {(onClose) => (
                    <>
                        <ModalHeader className="flex items-center justify-between pointer-events-none">
                            <span className="text-primary font-bold">Danh sách Quà tặng & Điều kiện Items</span>
                            <Button
                                size="sm"
                                startContent={<Plus size={16} />}
                                color="primary"
                                onPress={handleAddItem}
                                className="mr-8 pointer-events-auto"
                            >
                                Thêm Item
                            </Button>
                        </ModalHeader>
                        <ModalBody className="pb-8">
                            <div className="space-y-3">
                                {items.map((item, index) => (
                                    <Card key={index} shadow="sm" className="border border-gray-100">
                                        <CardBody className="p-3 grid grid-cols-12 gap-3 items-center">
                                            <div className="col-span-6">
                                                <Select
                                                    label="Sản phẩm / Biến thể"
                                                    size="sm"
                                                    variant="bordered"
                                                    placeholder="Chọn..."
                                                    selectedKeys={item.variant_id ? [item.variant_id] : []}
                                                    onChange={(e) => handleUpdateItem(index, "variant_id", e.target.value)}
                                                >
                                                    {variantOptions.map((opt) => (
                                                        <SelectItem key={opt.id} textValue={opt.label}>
                                                            {opt.label}
                                                        </SelectItem>
                                                    ))}
                                                </Select>
                                            </div>
                                            <div className="col-span-2">
                                                <Input
                                                    label="Số lượng"
                                                    type="number"
                                                    size="sm"
                                                    variant="bordered"
                                                    value={String(item.quantity)}
                                                    onValueChange={(val) => handleUpdateItem(index, "quantity", Number(val))}
                                                />
                                            </div>
                                            <div className="col-span-3">
                                                <Select
                                                    label="Loại"
                                                    size="sm"
                                                    variant="bordered"
                                                    selectedKeys={[String(item.type)]}
                                                    onChange={(e) => handleUpdateItem(index, "type", Number(e.target.value))}
                                                >
                                                    {voucherItemTypes.map((t) => (
                                                        <SelectItem key={String(t.value)}>
                                                            {t.label}
                                                        </SelectItem>
                                                    ))}
                                                </Select>
                                            </div>
                                            <div className="col-span-1 flex justify-center">
                                                <Button
                                                    isIconOnly
                                                    size="sm"
                                                    color="danger"
                                                    variant="light"
                                                    onPress={() => handleRemoveItem(index)}
                                                >
                                                    <Trash2 size={18} />
                                                </Button>
                                            </div>
                                        </CardBody>
                                    </Card>
                                ))}
                                {items.length === 0 && (
                                    <div className="text-center py-10 bg-gray-50 rounded-xl border-2 border-dashed border-gray-200">
                                        <p className="text-gray-400 text-sm">Chưa có item nào. Nhấn "Thêm Item" để bắt đầu.</p>
                                    </div>
                                )}
                            </div>
                        </ModalBody>
                        <ModalFooter className="bg-gray-50/50">
                            <Button color="secondary" variant="flat" onPress={onClose}>
                                Đóng
                            </Button>
                            <Button color="primary" onPress={handleSave} startContent={<Save size={18} />}>
                                Lưu thay đổi
                            </Button>
                        </ModalFooter>
                    </>
                )}
            </ModalContent>
        </Modal>
    )
}
