import { useMemo } from "react"
import { Button, Card, CardBody, Select, SelectItem } from "@heroui/react"
import { Plus, Trash2, Info, Gift } from "lucide-react"
import { useFormikContext } from "formik"
import { CreateVoucherItemReq, CreateVoucherRequest } from "@/features/vouchers/models/request"
import { VoucherItemType } from "@/features/vouchers/constants"
import { useGetAllProducts } from "@/features/products/hooks/useProduct"
import { InputStyled } from "@/components"

export const VoucherGiftManager = () => {
    const { values, setFieldValue } = useFormikContext<CreateVoucherRequest>()

    const { data: productsData } = useGetAllProducts({
        params: {},
        pagination: { page: 1, pageSize: 100 }
    })

    const variantOptions = useMemo(
        () =>
            productsData?.items?.flatMap((p) =>
                p.variants.map((v) => ({
                    sku: v.sku,
                    label: `${p.name} - ${v.sku}`
                }))
            ) || [],
        [productsData]
    )

    const addItem = (type: VoucherItemType) => {
        setFieldValue("voucherItems", [
            ...(values.voucherItems || []),
            { sku: "", quantity: 1, type }
        ])
    }

    const removeItem = (realIndex: number) => {
        const newItems = [...(values.voucherItems || [])]
        newItems.splice(realIndex, 1)
        setFieldValue("voucherItems", newItems)
    }

    const renderItemCard = (
        item: CreateVoucherItemReq,
        realIndex: number,
        theme: "default" | "warning"
    ) => {
        const borderClass = theme === "warning" ? "border-warning-200" : "border-default-200"
        const bgClass = theme === "warning" ? "bg-white" : "bg-white"

        return (
            <Card
                key={realIndex}
                shadow="none"
                className={`border shadow-sm ${borderClass} ${bgClass}`}
            >
                <CardBody className="p-3 space-y-2">
                    <Select
                        label="Chọn sản phẩm"
                        size="sm"
                        variant="flat"
                        selectedKeys={item.sku ? [item.sku] : []}
                        onChange={(e) =>
                            setFieldValue(`voucherItems.${realIndex}.sku`, e.target.value)
                        }
                    >
                        {variantOptions.map((opt) => (
                            <SelectItem key={opt.sku} textValue={opt.label}>
                                {opt.label}
                            </SelectItem>
                        ))}
                    </Select>
                    <div className="flex items-center gap-2">
                        <InputStyled
                            label="Số lượng"
                            type="number"
                            size="sm"
                            value={String(item.quantity)}
                            onValueChange={(v) =>
                                setFieldValue(`voucherItems.${realIndex}.quantity`, Number(v))
                            }
                        />
                        <Button
                            isIconOnly
                            size="sm"
                            color="danger"
                            variant="light"
                            onPress={() => removeItem(realIndex)}
                        >
                            <Trash2 size={16} />
                        </Button>
                    </div>
                </CardBody>
            </Card>
        )
    }

    return (
        <div className="mt-8 pt-8 border-t border-dashed border-default-200">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                {/* --- CỘT TRÁI: ĐIỀU KIỆN YÊU CẦU --- */}
                <div className="space-y-4">
                    <div className="flex justify-between items-center">
                        <div className="flex items-center gap-2">
                            <div className="w-8 h-8 rounded-lg bg-default-100 flex items-center justify-center text-default-600">
                                <Info size={16} />
                            </div>
                            <h3 className="text-sm font-black uppercase tracking-wider text-default-700">
                                Sản phẩm yêu cầu
                            </h3>
                        </div>
                        <Button
                            size="sm"
                            variant="flat"
                            startContent={<Plus size={14} />}
                            onPress={() => addItem(VoucherItemType.ITEM_REQUIRED)}
                        >
                            Thêm
                        </Button>
                    </div>
                    <div className="space-y-3 min-h-41 p-4 bg-default-50/50 rounded-2xl border-2 border-dashed border-default-200">
                        {values.voucherItems
                            ?.filter((i) => i.type === VoucherItemType.ITEM_REQUIRED)
                            .map((item) => {
                                const realIndex = values.voucherItems!.findIndex((v) => v === item)
                                return renderItemCard(item, realIndex, "default")
                            })}
                        {values.voucherItems?.filter(
                            (i) => i.type === VoucherItemType.ITEM_REQUIRED
                        ).length === 0 && (
                            <p className="text-[16px] text-center text-default-500 py-4 italic font-medium">
                                Chưa có điều kiện sản phẩm.
                            </p>
                        )}
                    </div>
                </div>

                {/* --- CỘT PHẢI: QUÀ TẶNG KÈM --- */}
                <div className="space-y-4">
                    <div className="flex justify-between items-center">
                        <div className="flex items-center gap-2">
                            <div className="w-8 h-8 rounded-lg bg-warning-100 flex items-center justify-center text-warning-600">
                                <Gift size={16} />
                            </div>
                            <h3 className="text-sm font-black uppercase tracking-wider text-warning-700">
                                Quà tặng kèm
                            </h3>
                        </div>
                        <Button
                            size="sm"
                            color="warning"
                            variant="flat"
                            startContent={<Plus size={14} />}
                            onPress={() => addItem(VoucherItemType.ITEM_GIFT)}
                        >
                            Thêm
                        </Button>
                    </div>
                    <div className="space-y-3 min-h-41 p-4 bg-warning-50/30 rounded-2xl border-2 border-dashed border-warning-200">
                        {values.voucherItems
                            ?.filter((i) => i.type === VoucherItemType.ITEM_GIFT)
                            .map((item) => {
                                const realIndex = values.voucherItems!.findIndex((v) => v === item)
                                return renderItemCard(item, realIndex, "warning")
                            })}
                        {values.voucherItems?.filter((i) => i.type === VoucherItemType.ITEM_GIFT)
                            .length === 0 && (
                            <p className="text-[16px] text-center text-warning-500 py-4 italic font-medium">
                                Chưa chọn quà tặng nào.
                            </p>
                        )}
                    </div>
                </div>
            </div>
        </div>
    )
}
