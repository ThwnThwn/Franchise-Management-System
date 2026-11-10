import { Tabs, Tab, Card, CardBody } from "@heroui/react"
import { Percent, Truck, Gift } from "lucide-react"
import { useFormikContext } from "formik"
import { CreateVoucherRequest } from "@/features/vouchers/models/request"
import { DatePickerStyled, InputStyled } from "@/components"
import { useDay } from "@/hooks"
import { VoucherTabType } from ".."

interface Props {
    uiTab: string
    setUiTab: (val: VoucherTabType) => void
    isPercent: boolean
    setIsPercent: (val: boolean) => void
    isEditing: boolean
}

export const VoucherBasicForm = ({
    uiTab,
    setUiTab,
    isPercent,
    setIsPercent,
    isEditing
}: Props) => {
    // Lấy mọi thứ từ Formik Context (Không cần truyền props rườm rà)
    const { values, handleChange, errors, setFieldValue } = useFormikContext<CreateVoucherRequest>()
    const { toDate, formatDate, dateNow } = useDay()

    // Ngày hiện tại làm mốc tối thiểu (không cho chọn quá khứ)
    const minDateStr = dateNow()

    return (
        <div className="grid grid-cols-12 gap-8">
            {/* ========================================== */}
            {/* CỘT TRÁI: CẤU HÌNH KHUYẾN MÃI VÀ THỜI GIAN */}
            {/* ========================================== */}
            <div className="col-span-12 lg:col-span-7 space-y-6">
                {/* 1. Chọn loại Khuyến mãi */}
                <div className="space-y-4">
                    <label className="text-[10px] font-black uppercase text-default-400 tracking-[0.2em]">
                        1. Loại hình khuyến mãi
                    </label>
                    <Tabs
                        fullWidth
                        variant="underlined"
                        selectedKey={uiTab}
                        onSelectionChange={(k) => setUiTab(k as VoucherTabType)}
                        color="warning"
                        classNames={{
                            tabList: "bg-default-100/50 p-1",
                            cursor: "shadow-md"
                        }}
                    >
                        <Tab
                            key="discount"
                            title={
                                <div className="flex items-center gap-2">
                                    <Percent size={16} />
                                    <span>Giảm giá</span>
                                </div>
                            }
                        />
                        <Tab
                            key="shipping"
                            title={
                                <div className="flex items-center gap-2">
                                    <Truck size={16} />
                                    <span>Giao hàng</span>
                                </div>
                            }
                        />
                        <Tab
                            key="gift"
                            title={
                                <div className="flex items-center gap-2">
                                    <Gift size={16} />
                                    <span>Quà tặng</span>
                                </div>
                            }
                        />
                    </Tabs>
                </div>

                {/* 2. Mã Code & Ngày hết hạn */}
                <div className="grid grid-cols-2 gap-4">
                    <InputStyled
                        name="code"
                        label="Mã Code"
                        variant="bordered"
                        placeholder="VD: COFFEE2026"
                        value={values.code}
                        isDisabled={isEditing}
                        onChange={handleChange}
                        isInvalid={!!errors.code}
                        errorMessage={errors.code as string}
                    />

                    <DatePickerStyled
                        label="Ngày hết hạn"
                        value={toDate(values.endDate)}
                        onChange={(date) => {
                            setFieldValue("endDate", formatDate(date))
                        }}
                        isInvalid={!!errors.endDate}
                        errorMessage={errors.endDate as string}
                        minValue={toDate(minDateStr)}
                    />
                </div>

                {/* 3. Khối giá trị giảm (Chỉ hiện khi không phải Quà tặng) */}
                {uiTab !== "gift" && (
                    <Card className="border border-warning-200/50 shadow-sm bg-white">
                        <CardBody className="p-5 space-y-5">
                            <div className="flex items-center justify-between">
                                <span className="text-sm font-bold text-default-700">
                                    Giá trị Voucher
                                </span>
                                <Tabs
                                    size="sm"
                                    radius="full"
                                    color="warning"
                                    selectedKey={isPercent ? "p" : "f"}
                                    onSelectionChange={(k) => setIsPercent(k === "p")}
                                >
                                    <Tab key="p" title="Theo %" />
                                    <Tab key="f" title="Tiền mặt" />
                                </Tabs>
                            </div>
                            <div className="grid grid-cols-2 gap-4">
                                <InputStyled
                                    name="discountValue"
                                    label="Mức giảm"
                                    type="number"
                                    variant="faded"
                                    value={values.discountValue}
                                    onChange={handleChange}
                                    isInvalid={!!errors.discountValue}
                                    errorMessage={errors.discountValue as string}
                                    endContent={
                                        <span className="text-xs font-bold text-default-400">
                                            {isPercent ? "%" : "đ"}
                                        </span>
                                    }
                                />

                                {isPercent && (
                                    <InputStyled
                                        name="maxDiscountValue"
                                        label="Giảm tối đa"
                                        type="number"
                                        variant="faded"
                                        value={values.maxDiscountValue}
                                        onChange={handleChange}
                                        endContent={
                                            <span className="text-xs font-bold text-default-400">
                                                đ
                                            </span>
                                        }
                                    />
                                )}
                            </div>
                        </CardBody>
                    </Card>
                )}
            </div>

            {/* ========================================== */}
            {/* CỘT PHẢI: ĐIỀU KIỆN ÁP DỤNG & SỐ LƯỢNG */}
            {/* ========================================== */}
            <div className="col-span-12 lg:col-span-5 space-y-5 lg:border-l lg:border-dashed lg:border-default-200 lg:pl-8">
                <label className="text-[10px] font-black uppercase text-default-400 tracking-[0.2em]">
                    2. Điều kiện áp dụng
                </label>

                <InputStyled
                    name="minOrderValue"
                    label="Đơn tối thiểu"
                    type="number"
                    variant="bordered"
                    value={values.minOrderValue}
                    onChange={handleChange}
                    endContent={<span className="text-xs font-bold text-default-400">đ</span>}
                />

                <InputStyled
                    name="pointCost"
                    label="Điểm đổi (Point Cost)"
                    type="number"
                    variant="bordered"
                    placeholder="Để trống nếu tặng miễn phí"
                    value={values.pointCost}
                    onChange={handleChange}
                />

                <div className="grid grid-cols-2 gap-4">
                    <InputStyled
                        name="initQuantity"
                        label="Số lượng"
                        type="number"
                        variant="bordered"
                        value={String(values.initQuantity)}
                        onChange={handleChange}
                        isInvalid={!!errors.initQuantity}
                        errorMessage={errors.initQuantity as string}
                    />
                    <InputStyled
                        name="maxUsagePerUser"
                        label="Lượt dùng / Khách"
                        type="number"
                        variant="bordered"
                        value={String(values.maxUsagePerUser)}
                        onChange={handleChange}
                        isInvalid={!!errors.maxUsagePerUser}
                        errorMessage={errors.maxUsagePerUser as string}
                    />
                </div>
            </div>
        </div>
    )
}
