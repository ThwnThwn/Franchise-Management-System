"use client"

import React, { useState, useCallback } from "react"
import { ModalBody, ModalContent, ModalHeader } from "@heroui/react"
import { ModalStyled, ImageStyled, ButtonStyled, NumberInputStyled } from "@/components"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import { useCart } from "@/features/cart"
import { GetProductListItemDto, ProductSizeListItemDto } from "../../models/schema/respone"

type ProductVariantsModalProps = {
    product: GetProductListItemDto | null
    isOpen: boolean
    onOpenChange: (open: boolean) => void
}

export function ProductVariantsModal({ product, isOpen, onOpenChange }: ProductVariantsModalProps) {
    const [selectedSize, setSelectedSize] = useState<ProductSizeListItemDto | null>(null)
    const [quantity, setQuantity] = useState<number>(1)
    const { addItem } = useCart()

    const handleClose = useCallback(() => {
        onOpenChange(false)
        setSelectedSize(null)
        setQuantity(1)
    }, [onOpenChange])

    const handleConfirm = useCallback(() => {
        if (!product || !selectedSize) return
        const safeQty = quantity > 0 ? quantity : 1

        // Add to cart using new cart store
        addItem({
            sku: selectedSize.sku,
            productId: product.id,
            variantId: selectedSize.id,
            productName: product.name,
            sizeName: selectedSize.name,
            attributes: selectedSize.attributes,
            basePrice: selectedSize.basePrice,
            imageUrl: product.imageUrl,
            quantity: safeQty
        })

        handleClose()
    }, [product, selectedSize, quantity, addItem, handleClose])

    if (!product) return null

    const sizes = product.sizes?.filter((s) => s.isActive) ?? []

    return (
        <ModalStyled isOpen={isOpen} onOpenChange={onOpenChange}>
            <ModalContent className="max-w-md py-4">
                <ModalHeader className="flex flex-col gap-1 pb-2">
                    <span className="text-lg font-semibold">{product.name}</span>
                    {product.description && (
                        <span className="text-small text-default-500 font-normal">
                            {product.description}
                        </span>
                    )}
                </ModalHeader>
                <ModalBody className="gap-4">
                    <ImageStyled
                        alt={product.name}
                        src={product.imageUrl}
                        className="object-cover rounded-xl w-full aspect-video"
                        width={400}
                        height={220}
                    />
                    <div>
                        <p className="text-small text-default-500 mb-2">Chọn size</p>
                        <div className="flex flex-wrap gap-2">
                            {sizes.map((size) => (
                                <button
                                    key={size.id}
                                    type="button"
                                    onClick={() => setSelectedSize(size)}
                                    className={`rounded-xl border-2 px-4 py-2 text-left transition-colors ${selectedSize?.id === size.id
                                        ? "border-primary bg-primary/10 text-primary"
                                        : "border-default-200 hover:border-primary/50"
                                        }`}
                                >
                                    <span className="font-medium">{size.name}</span>
                                    {size.attributes && (
                                        <span className="text-default-500 text-small block">
                                            {size.attributes}
                                        </span>
                                    )}
                                    <span className="text-primary font-semibold">
                                        {formatCurrencyWithSymbol(size.basePrice)}
                                    </span>
                                </button>
                            ))}
                        </div>
                    </div>
                    <div className="flex items-center justify-between gap-4">
                        <p className="text-small text-default-500">Số lượng</p>
                        <NumberInputStyled
                            min={1}
                            value={quantity}
                            onValueChange={(value) => {
                                const n = Number(value)
                                setQuantity(!Number.isNaN(n) && n > 0 ? n : 1)
                            }}
                            className="w-24"
                        />
                    </div>
                    <ButtonStyled
                        isDisabled={!selectedSize || quantity <= 0}
                        color="primary"
                        onPress={handleConfirm}
                        className="w-full"
                    >
                        Thêm vào giỏ hàng
                    </ButtonStyled>
                </ModalBody>
            </ModalContent>
        </ModalStyled>
    )
}
