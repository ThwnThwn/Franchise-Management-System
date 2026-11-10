"use client"

import { Card, CardBody, AutocompleteItem, CardHeader } from "@heroui/react"
import { RotateCcw, Search } from "lucide-react"
import {
    AutocompleteStyled,
    ButtonStyled,
    DatePickerStyled,
    EnumPicker,
    InputStyled
} from "@/components"
import { GetOrdersQuery } from "@/features/orders/models/request"
import { OrderStatusLabels, PaymentMethodLabels } from "@/features/orders/constants"

import { StoreDetailDto } from "@/features/stores"
import { useDay, useTokenStore } from "@/hooks"
import { DEFAULT_DATE_FORMAT } from "@/constants/constants"
import { RoleSlug } from "@/constants/enums"
import { useState } from "react"

interface Props {
    filter: GetOrdersQuery
    onFilterChange: (newFilter: GetOrdersQuery) => void // Dùng cho các dropdown (không debounce)
    debouncedSearch: (params: GetOrdersQuery) => void // Dùng riêng cho input Code
    stores: StoreDetailDto[]
    scrollRef?: (node?: Element | null) => void
}

export const OrdersToolBar = ({
    filter,
    onFilterChange,
    debouncedSearch,
    stores,
    scrollRef
}: Props) => {
    const { toDate, formatDate } = useDay({ defaultFormat: DEFAULT_DATE_FORMAT })
    const isAdmin = useTokenStore((s) => s.roleSlug === RoleSlug.Admin)
    const storeIdFromToken = useTokenStore((s) => s.storeId) || undefined

    const [resetKey, setResetKey] = useState(0)

    const handleCodeChange = (val: string) => {
        debouncedSearch({ ...filter, code: val })
    }

    const handleReset = () => {
        setResetKey((prev) => prev + 1)

        onFilterChange({
            storeId: storeIdFromToken,
            code: "",
            status: undefined,
            paymentMethod: undefined,
            fromDate: "",
            toDate: ""
        })
    }

    return (
        <Card className="shadow-none border border-divider bg-white/50 backdrop-blur-md">
            <CardHeader className="px-3 py-2 flex justify-between">
                <p className="font-medium">Bộ lọc</p>
                <ButtonStyled isIconOnly variant="ghost" onPress={handleReset}>
                    <RotateCcw size={18} />
                </ButtonStyled>
            </CardHeader>
            <CardBody className="px-3 py-2 space-y-3">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                    <InputStyled
                        key={`store-${resetKey}`}
                        isClearable
                        label="Mã đơn"
                        placeholder="Nhập mã đơn..."
                        startContent={<Search size={16} className="text-default-400" />}
                        onValueChange={handleCodeChange}
                        onClear={() => handleCodeChange("")}
                    />
                    {isAdmin && (
                        <div>
                            <AutocompleteStyled
                                key={`store-${resetKey}`}
                                label="Cửa hàng"
                                items={stores}
                                selectedKey={filter.storeId}
                                onSelectionChange={(id) => {
                                    onFilterChange({ ...filter, storeId: id as string | undefined })
                                }}
                                listboxProps={{
                                    emptyContent: "---"
                                }}
                            >
                                {(stores ?? []).map((item) => (
                                    <AutocompleteItem key={item.id}>
                                        {`${item.name} - ${item.address}`}
                                    </AutocompleteItem>
                                ))}
                            </AutocompleteStyled>
                            <div ref={scrollRef} className="h-0" />
                        </div>
                    )}
                </div>
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-3">
                    <EnumPicker
                        label={"Trạng thái"}
                        labels={OrderStatusLabels}
                        value={filter.status}
                        onChange={(val) => onFilterChange({ ...filter, status: val })}
                    />

                    <EnumPicker
                        label={"Thanh toán"}
                        labels={PaymentMethodLabels}
                        value={filter.paymentMethod}
                        onChange={(val) => onFilterChange({ ...filter, paymentMethod: val })}
                    />

                    <DatePickerStyled
                        label="Từ ngày"
                        value={toDate(filter.fromDate)}
                        onChange={(date) =>
                            onFilterChange({ ...filter, fromDate: formatDate(date) })
                        }
                    />
                    <DatePickerStyled
                        label="Đến ngày"
                        value={toDate(filter.toDate)}
                        onChange={(date) => onFilterChange({ ...filter, toDate: formatDate(date) })}
                    />
                </div>
            </CardBody>
        </Card>
    )
}
