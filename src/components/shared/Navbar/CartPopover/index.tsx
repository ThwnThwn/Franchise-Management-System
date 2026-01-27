"use client"

import { DropdownTrigger, DropdownMenu, DropdownItem } from "@heroui/react"
import { DropdownStyled, ButtonIconStyled, ButtonStyled } from "@/components"
import { ShoppingCart, X } from "lucide-react"
import { STRINGS } from "@/constants/strings"
import { useCartStore } from "@/hooks"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"

export function CartPopover() {
    const items = useCartStore((s) => s.items)
    const removeItem = useCartStore((s) => s.removeItem)
    const isEmpty = items.length === 0

    const totalQuantity = items.reduce((sum, item) => sum + item.quantity, 0)
    const totalPrice = items.reduce((sum, item) => sum + item.basePrice * item.quantity, 0)

    return (
        <DropdownStyled classNames={{ content: "min-w-56 max-w-80" }}>
            <DropdownTrigger>
                <ButtonIconStyled className="z-9999" aria-label="Giỏ hàng">
                    <ShoppingCart size={24} />
                </ButtonIconStyled>
            </DropdownTrigger>
            <DropdownMenu aria-label="Giỏ hàng" disabledKeys={isEmpty ? ["empty"] : []}>
                {isEmpty ? (
                    <DropdownItem
                        key="empty"
                        isDisabled
                        className="cursor-default justify-center py-4"
                    >
                        {STRINGS.NAVBAR.CART_EMPTY}
                    </DropdownItem>
                ) : (
                    <>
                        {items.map((item, i) => (
                            <DropdownItem
                           
                                key={`${item.productId}-${item.sizeId}-${i}`}
                                className="cursor-default py-3 px-4 "
                                textValue={item.productName}
                            >
                                <div className="flex items-start gap-2">
                                    <div className="flex flex-col gap-0.5 flex-1">
                                        <span className="font-medium">{item.productName}</span>
                                        <span className="text-small text-default-500">
                                            Size {item.sizeName} × {item.quantity} —{" "}
                                            {formatCurrencyWithSymbol(
                                                item.basePrice * item.quantity
                                            )}
                                        </span>
                                    </div>
                                    <button
                                        type="button"
                                        className="text-default-400 hover:text-danger shrink-0"
                                        onClick={(e) => {
                                            e.preventDefault()
                                            e.stopPropagation()
                                            removeItem(item.productId, item.sizeId)
                                        }}
                                        aria-label="Xóa sản phẩm khỏi giỏ"
                                    >
                                        <X size={16} />
                                    </button>
                                </div>
                            </DropdownItem>
                        ))}
                        <DropdownItem
                            key="cart-summary"
                            className="cursor-default pointer-events-none border-t border-default-200 mt-1 pt-3"
                        >
                            <div className="flex flex-col gap-2">
                                <div className="flex flex-col gap-1">
                                    <span className="text-small text-default-500">
                                        Tổng số lượng:{" "}
                                        <span className="font-semibold text-foreground">
                                            {totalQuantity}
                                        </span>
                                    </span>
                                    <span className="text-small text-default-500">
                                        Tổng tiền:{" "}
                                        <span className="font-semibold text-primary">
                                            {formatCurrencyWithSymbol(totalPrice)}
                                        </span>
                                    </span>
                                </div>
                                <ButtonStyled
                                    color="primary"
                                    className="w-full mt-1 pointer-events-auto"
                                    onPress={() => {
                                        // TODO: điều hướng tới trang thanh toán
                                        console.log("Go to checkout")
                                    }}
                                >
                                    Thanh toán
                                </ButtonStyled>
                            </div>
                        </DropdownItem>
                    </>
                )}
            </DropdownMenu>
        </DropdownStyled>
    )
}
