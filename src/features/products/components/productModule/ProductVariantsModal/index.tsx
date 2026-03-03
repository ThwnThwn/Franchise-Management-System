"use client"

import React, { useState, useCallback } from "react"
import { ModalBody, ModalContent, ModalHeader } from "@heroui/react"
import { ModalStyled, ImageStyled, ButtonStyled, NumberInputStyled } from "@/components"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import { useCartStore } from "@/features/cart/store"
import { parseAttributes } from "@/features/products/utils/utils"
import { useTokenStore } from "@/hooks"
import { useAddToCart } from "@/features/cart/hooks/useCartQuery"
import { Product, ProductVariant } from "@/features/products/models/products/respone"
import { RoleSlug } from "@/constants/enums"

type ProductVariantsModalProps = {
    product: Product | null
    isOpen: boolean
    onOpenChange: () => void
    storeId?: string
}

export function ProductVariantsModal({
    product,
    isOpen,
    onOpenChange,
    storeId
}: ProductVariantsModalProps) {
    const isCustomer = useTokenStore((s) => s.roleSlug === RoleSlug.Customer)
    const [selectedSize, setSelectedSize] = useState<ProductVariant | null>(null)
    const [quantity, setQuantity] = useState<number>(1)
    const addItem = useCartStore((state) => state.addItem)

    const isLogined = useTokenStore((s) => !!s.accessToken)
    const addToCartMutation = useAddToCart()

    const handleOpenChange = useCallback(() => {
        setSelectedSize(null)
        onOpenChange()
    }, [onOpenChange])

    const handleClose = useCallback(() => {
        onOpenChange()
        setSelectedSize(null)
        setQuantity(1)
    }, [onOpenChange])

    const handleConfirm = useCallback(() => {
        if (!product || !selectedSize) return
        const safeQty = quantity > 0 ? quantity : 1

        if (isLogined) {
            addToCartMutation.mutate({
                variantId: selectedSize.id,
                quantity: safeQty,
                storeId: storeId,
                sku: selectedSize.sku
            })
        } else {
            // Fallback for guest or if API fails (though mutation handles success/error)
            // Add to cart using local cart store
            addItem({
                sku: selectedSize.sku,
                // productId: product.id,
                // variantId: selectedSize.id,
                productName: product.name,
                imageUrl: product.imageUrl,
                attributes: selectedSize.attributes,
                // sizeName: selectedSize.name,
                basePrice: selectedSize.basePrice,
                quantity: safeQty
                // storeId: storeId
            })
        }

        handleClose()
    }, [
        product,
        selectedSize,
        quantity,
        addItem,
        handleClose,
        storeId,
        isLogined,
        addToCartMutation
    ])

    if (!product) return null

    const variants = product.variants?.filter((s) => s.isActive) ?? []

    return (
        <ModalStyled
            isDismissable
            id={selectedSize?.id}
            isOpen={isOpen}
            onOpenChange={handleOpenChange}
        >
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
                        className="mx-auto w-96 h-48"
                    />
                    <div>
                        <p className="text-small text-default-500 mb-2">Chọn size</p>
                        <div className="flex flex-wrap gap-2">
                            {variants
                                .sort((a, b) => b.basePrice - a.basePrice)
                                .map((size) => {
                                    const attrs = parseAttributes(size.attributes)
                                    return (
                                        <ButtonStyled
                                            key={size.id}
                                            variant="ghost"
                                            onPress={() => setSelectedSize(size)}
                                            className={`flex flex-col gap-0 min-h-fit w-full items-start px-3 py-2 ${
                                                selectedSize?.id === size.id &&
                                                "border-primary bg-primary/10 text-primary"
                                            }`}
                                        >
                                            <div className="w-full flex justify-between">
                                                <span className="font-medium">{size.sku}</span>
                                                <span className="text-primary font-semibold">
                                                    {formatCurrencyWithSymbol(size.basePrice)}
                                                </span>
                                            </div>
                                            {size.attributes && (
                                                <div className="flex gap-2">
                                                    {attrs.map((attr) => (
                                                        <span
                                                            key={attr.originalKey}
                                                            className="px-2 py-1 text-xs bg-gray-100 rounded text-gray-600"
                                                        >
                                                            {/* Hiển thị kiểu: Size: L */}
                                                            <span className="font-semibold">
                                                                {attr.key}:
                                                            </span>{" "}
                                                            {attr.value}
                                                        </span>
                                                    ))}
                                                </div>
                                            )}
                                        </ButtonStyled>
                                    )
                                })}
                        </div>
                    </div>
                    <div className="flex items-center justify-between gap-4" hidden={!isCustomer}>
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
                        isDisabled={!selectedSize?.id || quantity <= 0}
                        color="primary"
                        onPress={handleConfirm}
                        className="w-full"
                        hidden={!isCustomer}
                    >
                        Thêm vào giỏ hàng
                    </ButtonStyled>
                </ModalBody>
            </ModalContent>
        </ModalStyled>
    )
}
