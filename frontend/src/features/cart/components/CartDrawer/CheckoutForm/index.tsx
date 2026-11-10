import { FormikProps } from "formik"
import { Clock, MapPin, FileText, Package } from "lucide-react"
import { EnumPicker, InputStyled, TextareaStyled } from "@/components"
import { useFormikHelpers } from "@/hooks"
import { CheckoutFormValues } from "../schema"
import { OrderType, OrderTypeLabel } from "@/features/orders"

interface CheckoutFormProps {
    formik: FormikProps<CheckoutFormValues>
}

export function CheckoutForm({ formik }: CheckoutFormProps) {
    const { isInvalid, getErrorMessage } = useFormikHelpers(formik)

    return (
        <div className="space-y-3 pt-5 border-t border-default-200">
            <h3 className="font-bold text-lg flex items-center gap-2">
                <Package size={20} className="text-primary" />
                Thông tin nhận hàng
            </h3>

            {/* 1. Hình thức nhận hàng (Bắt buộc) */}
            <EnumPicker<OrderType>
                label="Hình thức nhận hàng"
                labels={OrderTypeLabel}
                value={formik.values.type}
                onChange={(val) => {
                    formik.setFieldValue("type", val)
                    // Reset lỗi địa chỉ nếu khách quay xe chọn "Đến lấy"
                    if (val === OrderType.Pickup) {
                        formik.setFieldError("address", undefined)
                    }
                }}
                errorMessage={getErrorMessage("type")}
                isInvalid={isInvalid("type")}
                startContent={<Package size={18} className="text-default-400" />}
            />

            {/* 2. Địa chỉ giao hàng (Chỉ hiện khi chọn Delivery) */}
            {formik.values.type === OrderType.Delivery && (
                <div className="animate-in fade-in slide-in-from-top-2 duration-300">
                    <InputStyled
                        label="Địa chỉ giao hàng"
                        placeholder="VD: 123 Đường ABC, Quận X..."
                        name="address"
                        value={formik.values.address}
                        onChange={formik.handleChange}
                        onBlur={formik.handleBlur}
                        errorMessage={getErrorMessage("address")}
                        isInvalid={isInvalid("address")}
                        isRequired
                        startContent={<MapPin size={18} className="text-default-400" />}
                    />
                </div>
            )}

            {/* 3. Thời gian hẹn lấy/giao (Optional) */}
            <InputStyled
                type="time" // Dùng type time để trên mobile nó bung cái đồng hồ quay quay cho dễ xài
                label="Hẹn giờ"
                placeholder="HH:mm"
                name="scheduledTime"
                value={formik.values.scheduledTime}
                onChange={formik.handleChange}
                onBlur={formik.handleBlur}
                errorMessage={getErrorMessage("scheduledTime")}
                isInvalid={isInvalid("scheduledTime")}
                startContent={<Clock size={18} className="text-default-400" />}
                description="Bỏ trống nếu bạn muốn nhận hàng ngay lập tức."
            />

            {/* 4. Ghi chú đơn hàng (Optional) */}
            <TextareaStyled
                label="Ghi chú đơn hàng"
                placeholder="VD: Ít đá, nhiều tương ớt, dặn shipper gọi trước khi tới..."
                name="notes"
                value={formik.values.notes}
                onChange={formik.handleChange}
                onBlur={formik.handleBlur}
                errorMessage={getErrorMessage("notes")}
                isInvalid={isInvalid("notes")}
                startContent={<FileText size={18} className="text-default-400 mt-1" />}
                minRows={2}
            />
        </div>
    )
}
