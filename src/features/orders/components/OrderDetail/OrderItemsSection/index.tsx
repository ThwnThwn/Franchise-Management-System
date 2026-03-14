import { Accordion, AccordionItem } from "@heroui/react"
import { OrderItemRow } from "../OrderItemRow"
import { RecipeIngredientList } from "@/features/products/components"
import { OrderItemDisplay } from "@/features/orders/types"
import { RecipeViewRes } from "@/features/products/models/recipes/response"

interface Props {
    purchasedItems: OrderItemDisplay[]
    giftItems: OrderItemDisplay[]
    isCustomer?: boolean
    recipes?: RecipeViewRes[]
    defaultExpandedKeys: string[]
}

export const OrderItemsSection = ({
    purchasedItems,
    giftItems,
    isCustomer,
    recipes = [],
    defaultExpandedKeys
}: Props) => {
    const renderContent = (items: OrderItemDisplay[], isGift: boolean) => {
        if (items.length === 0) return null

        if (isCustomer) {
            return (
                <div className="flex flex-col">
                    {items.map((item) => (
                        <div
                            key={`${isGift ? "gift" : "buy"}-${item.id}`}
                            className="px-2 py-4 border-b border-default-100 last:border-none"
                        >
                            <OrderItemRow
                                item={item}
                                isGift={isGift}
                                overrideQuantity={item.displayQuantity}
                            />
                        </div>
                    ))}
                </div>
            )
        }

        return (
            <Accordion
                selectionMode="multiple"
                variant="light"
                className="px-0"
                defaultExpandedKeys={defaultExpandedKeys}
                hideIndicator
                itemClasses={{
                    base: "border-b border-default-100 last:border-none",
                    title: "p-0",
                    trigger: "px-2 py-4 rounded-xl hover:bg-default-50 transition-colors",
                    content: "px-4 pt-0 pb-4"
                }}
            >
                {items.map((item) => (
                    <AccordionItem
                        key={`${isGift ? "gift" : "buy"}-${item.id}`}
                        aria-label={item.productName}
                        title={
                            <OrderItemRow
                                item={item}
                                isGift={isGift}
                                overrideQuantity={item.displayQuantity}
                            />
                        }
                    >
                        <RecipeIngredientList variantId={item.variantId} recipes={recipes} />
                    </AccordionItem>
                ))}
            </Accordion>
        )
    }

    return (
        <div className="bg-white rounded-2xl border border-default-200 shadow-sm overflow-hidden">
            <div className="grid grid-cols-12 px-4 py-3 bg-default-50 border-b border-default-200">
                <div className="col-span-6 text-[10px] uppercase font-bold tracking-widest text-default-500">
                    Sản phẩm
                </div>
                <div className="col-span-2 text-[10px] uppercase font-bold tracking-widest text-default-500 text-center">
                    Đơn giá
                </div>
                <div className="col-span-2 text-[10px] uppercase font-bold tracking-widest text-default-500 text-center">
                    SL
                </div>
                <div className="col-span-2 text-[10px] uppercase font-bold tracking-widest text-default-500 text-right">
                    Thành tiền
                </div>
            </div>

            {renderContent(purchasedItems, false)}

            {giftItems.length > 0 && (
                <>
                    <div className="bg-success-50/50 px-4 py-2 border-y border-success-100 text-xs font-bold text-success-700 flex items-center gap-2">
                        Quà tặng kèm
                    </div>
                    {renderContent(giftItems, true)}
                </>
            )}
        </div>
    )
}
