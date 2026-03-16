import { OrderType } from "@/features/orders"
import dayjs from "dayjs"
import * as yup from "yup"

export const checkoutSchema = yup.object({
    type: yup
        .mixed<OrderType>()
        .oneOf(Object.values(OrderType) as OrderType[])
        .required("Vui lòng chọn hình thức nhận hàng"),

    address: yup.string().when("type", {
        is: OrderType.Delivery,
        then: (schema) => schema.required("Vui lòng nhập địa chỉ giao hàng"),
        otherwise: (schema) => schema.notRequired().nullable()
    }),

    notes: yup.string().max(500, "Ghi chú không được vượt quá 500 ký tự").optional(),

    // Validate giờ HH:mm (Tùy chọn)
    scheduledTime: yup
        .string()
        .matches(/^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$/, {
            message: "Định dạng giờ không hợp lệ (HH:mm)",
            excludeEmptyString: true
        })
        .test("is-valid-time", "Thời gian đặt trước phải sau hiện tại ít nhất 30 phút", (value) => {
            // Cho phép bỏ trống (Giao ngay)
            if (!value) return true

            // Tách giờ và phút từ chuỗi "HH:mm"
            const [hours, minutes] = value.split(":")

            // Tạo object thời gian của thời điểm user chọn (mặc định lấy ngày hôm nay)
            const scheduledTime = dayjs().hour(Number(hours)).minute(Number(minutes)).second(0)

            // Tính thời gian tối thiểu được phép chọn (Hiện tại + 30 phút)
            const minTime = dayjs().add(30, "minute")

            // Check: Giờ chọn phải LỚN HƠN hoặc BẰNG (Hiện tại + 30p)
            // (Vì regex HH:mm đã chặn việc nhập qua ngày hôm sau nên không sợ nhập lố 24h)
            return scheduledTime.isAfter(minTime) || scheduledTime.isSame(minTime)
        })
        .optional()
})

// Xuất type để dùng cho Formik
export type CheckoutFormValues = yup.InferType<typeof checkoutSchema>
