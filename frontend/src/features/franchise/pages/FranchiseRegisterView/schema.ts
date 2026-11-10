import * as Yup from "yup"

export const SubmitFranchiseSchema = Yup.object().shape({
    fullName: Yup.string().required("Vui lòng nhập họ và tên"),
    email: Yup.string().email("Email không hợp lệ").required("Vui lòng nhập email"),
    phone: Yup.string()
        .matches(/^[0-9]{10}$/, "Số điện thoại phải bao gồm 10 chữ số")
        .required("Vui lòng nhập số điện thoại"),
    identityNumber: Yup.string()
        .matches(/^[0-9]{9,12}$/, "CMND/CCCD không hợp lệ (9-12 số)")
        .required("Vui lòng nhập số CMND/CCCD"),

    // Địa chỉ hành chính
    provinceId: Yup.string().required("Vui lòng chọn Tỉnh/Thành phố"),
    districtId: Yup.string().required("Vui lòng chọn Quận/Huyện"),
    wardId: Yup.string().required("Vui lòng chọn Phường/Xã"),

    // Thông tin mặt bằng
    storeName: Yup.string().required("Vui lòng nhập tên quán dự kiến"),
    storeAddress: Yup.string().required("Vui lòng nhập địa chỉ mặt bằng cụ thể"),
    storeSizeSqm: Yup.number()
        .min(20, "Diện tích mặt bằng tối thiểu phải từ 20m2")
        .required("Vui lòng nhập diện tích mặt bằng"),

    // File Uploads
    storeFront: Yup.mixed().required("Vui lòng tải lên 1 ảnh mặt tiền"),
    landCerts: Yup.array()
        .min(1, "Vui lòng tải lên ít nhất 1 ảnh chứng nhận đất đai/sổ đỏ")
        .required("Vui lòng tải lên ảnh sổ đỏ")
})
