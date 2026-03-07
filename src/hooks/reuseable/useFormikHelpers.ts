import { FormikProps, getIn } from "formik"

export const useFormikHelpers = <T>(formik: FormikProps<T>) => {
    /**
     * Kiểm tra field có bị lỗi và đã được touch chưa.
     * Hỗ trợ cả string path: "abc.0.name"
     */
    const isInvalid = (path: string): boolean => {
        const error = getIn(formik.errors, path)
        const touched = getIn(formik.touched, path)
        return !!error && !!touched
    }

    /**
     * Lấy message lỗi nếu field không hợp lệ
     */
    const getErrorMessage = (path: string): string | undefined => {
        return isInvalid(path) ? getIn(formik.errors, path) : undefined
    }

    /**
     * Xử lý ép kiểu số an toàn từ Input (string) và gán trực tiếp vào Formik.
     * @param field Tên field trong Formik
     * @param value Giá trị chuỗi người dùng nhập vào
     * @param fallback Giá trị gán vào khi người dùng xóa trắng (Mặc định là 0, có thể truyền "")
     */
    const setNumberValue = (field: string, value: string, fallback: number | string = 0) => {
        // 1. Xóa trắng
        if (value === "") {
            formik.setFieldValue(field, fallback)
            return
        }

        // 2. Ép kiểu
        const parsedValue = Number(value)

        // 3. Nếu nhập bậy (chữ, ký tự lạ) sinh ra NaN -> Bỏ qua không set
        if (Number.isNaN(parsedValue)) {
            return
        }

        // 4. Hợp lệ -> Set vào Formik
        formik.setFieldValue(field, parsedValue)
    }

    return {
        isInvalid,
        getErrorMessage,
        setNumberValue
    }
}
