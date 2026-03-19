import * as Yup from "yup"

export const CampaignSchema = Yup.object().shape({
    name: Yup.string().trim().required("Vui lòng nhập tên chiến dịch/sự kiện"),
    description: Yup.string().trim(),
    startDate: Yup.string().required("Vui lòng chọn ngày bắt đầu"),
    endDate: Yup.string()
        .required("Vui lòng chọn ngày kết thúc")
        .test("is-greater", "Ngày kết thúc phải diễn ra sau ngày bắt đầu", function (value) {
            const { startDate } = this.parent
            if (!startDate || !value) return true
            return new Date(value) > new Date(startDate)
        }),
    voucherIds: Yup.array().of(Yup.string()).default([])
})
