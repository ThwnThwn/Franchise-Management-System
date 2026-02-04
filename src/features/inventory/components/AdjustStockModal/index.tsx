"use client"

import React, { useState, useEffect } from "react"
import {
    ModalStyled,
    InputStyled,
    ButtonStyled
} from "@/components"
import {
    ModalContent,
    ModalHeader,
    ModalBody,
    ModalFooter,
    Button
} from "@heroui/react"
import { StockItem } from "../../models/response"

type AdjustStockModalProps = {
    isOpen: boolean
    onClose: () => void
    stock: StockItem | null
    type: "increase" | "decrease"
    onSubmit: (quantity: number) => void
    isLoading?: boolean
}

export function AdjustStockModal({
    isOpen,
    onClose,
    stock,
    type,
    onSubmit,
    isLoading = false
}: AdjustStockModalProps) {
    const [quantity, setQuantity] = useState("")
    const [error, setError] = useState("")

    useEffect(() => {
        if (isOpen) {
            setQuantity("")
            setError("")
        }
    }, [isOpen])

    const handleSubmit = () => {
        const qty = parseFloat(quantity)

        if (isNaN(qty) || qty <= 0) {
            setError("Vui lòng nhập số lượng hợp lệ")
            return
        }

        // Check if decreasing more than available
        if (type === "decrease") {
            const currentQty = parseFloat(stock?.quantity || "0")
            if (qty > currentQty) {
                setError(`Số lượng xuất không được vượt quá ${currentQty}`)
                return
            }
        }

        onSubmit(qty)
    }

    if (!stock) return null

    const currentQty = parseFloat(stock.quantity)
    const adjustedQty =
        type === "increase" ? currentQty + parseFloat(quantity || "0") : currentQty - parseFloat(quantity || "0")

    return (
        <ModalStyled isOpen={isOpen} onClose={onClose} size="md">
            <ModalContent>
                <ModalHeader className="flex flex-col gap-1">
                    {type === "increase" ? "Nhập kho" : "Xuất kho"}
                </ModalHeader>
                <ModalBody>
                    <div className="space-y-4">
                        {/* Nguyên liệu info */}
                        <div className="bg-slate-50 p-4 rounded-lg">
                            <div className="text-sm text-slate-600 mb-1">Nguyên liệu</div>
                            <div className="font-semibold text-slate-900">{stock.ingredient.name}</div>
                            <div className="text-xs text-slate-500 mt-1">Đơn vị: {stock.ingredient.baseUnit}</div>
                        </div>

                        {/* Số lượng hiện tại */}
                        <div className="flex justify-between items-center">
                            <span className="text-sm text-slate-600">Số lượng hiện tại:</span>
                            <span className="font-semibold">
                                {currentQty.toFixed(2)} {stock.ingredient.baseUnit}
                            </span>
                        </div>

                        {/* Input số lượng: Dùng InputStyled thay vì Input */}
                        <InputStyled
                            type="number"
                            label={type === "increase" ? "Số lượng nhập" : "Số lượng xuất"}
                            placeholder="Nhập số lượng"
                            value={quantity}
                            onChange={(e) => {
                                setQuantity(e.target.value)
                                setError("")
                            }}
                            min={0}
                            step={0.01}
                            isInvalid={!!error}
                            errorMessage={error}
                            endContent={
                                <div className="text-xs text-slate-400">{stock.ingredient.baseUnit}</div>
                            }
                        />

                        {/* Số lượng sau điều chỉnh */}
                        {quantity && !error && (
                            <div className="flex justify-between items-center bg-blue-50 p-3 rounded-lg">
                                <span className="text-sm text-blue-900">Số lượng sau điều chỉnh:</span>
                                <span className="font-bold text-blue-900">
                                    {adjustedQty.toFixed(2)} {stock.ingredient.baseUnit}
                                </span>
                            </div>
                        )}
                    </div>
                </ModalBody>
                <ModalFooter>
                    <Button color="danger" variant="light" onPress={onClose} disabled={isLoading}>
                        Hủy
                    </Button>
                    <ButtonStyled
                        color={type === "increase" ? "success" : "warning"}
                        onPress={handleSubmit}
                        isLoading={isLoading}
                    >
                        {type === "increase" ? "Nhập kho" : "Xuất kho"}
                    </ButtonStyled>
                </ModalFooter>
            </ModalContent>
        </ModalStyled>
    )
}
