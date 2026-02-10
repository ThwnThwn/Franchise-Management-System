import { FormikProps } from "formik"

export const useFormikHelpers = <T>(formik: FormikProps<T>) => {
    /**
     * Kiểm tra field có bị lỗi và đã được touch chưa
     */
    const isInvalid = (field: keyof T): boolean => {
        return !!formik.errors[field] && !!formik.touched[field]
    }

    /**
     * Lấy message lỗi nếu field không hợp lệ
     */
    const getErrorMessage = (field: keyof T): string | undefined => {
        return isInvalid(field) ? (formik.errors[field] as string) : undefined
    }

    return {
        isInvalid,
        getErrorMessage
    }
}
