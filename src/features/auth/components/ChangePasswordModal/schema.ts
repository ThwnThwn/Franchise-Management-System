import * as Yup from "yup"

export const ChangePasswordSchema = Yup.object().shape({
    oldPassword: Yup.string().required("Vui lòng nhập mật khẩu hiện tại"),
    newPassword: Yup.string()
        .notOneOf([Yup.ref("oldPassword")], "Mật khẩu mới không được trùng mật khẩu cũ")
        .required("Vui lòng nhập mật khẩu mới"),
    confirmNewPassword: Yup.string()
        .oneOf([Yup.ref("newPassword")], "Mật khẩu xác nhận không khớp")
        .required("Vui lòng xác nhận mật khẩu mới"),
    forgetAll: Yup.boolean().default(false)
})
