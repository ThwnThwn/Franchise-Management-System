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

    return {
        isInvalid,
        getErrorMessage
    }
}
