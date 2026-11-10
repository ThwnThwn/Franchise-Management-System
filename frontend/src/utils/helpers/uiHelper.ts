export const getPixelSizeFromClass = (className?: string) => {
    if (!className) return { width: 0, height: 0 }

    const widthMatch = className.match(/\bw-([0-9.]+)\b/)
    const heightMatch = className.match(/\bh-([0-9.]+)\b/)

    const width = widthMatch ? parseFloat(widthMatch[1]) * 4 : 0
    const height = heightMatch ? parseFloat(heightMatch[1]) * 4 : 0

    return { width, height }
}

/**
 * Loại bỏ một hoặc nhiều keys khỏi một object/record và trả về object mới.
 * @param obj Object gốc (ví dụ: PaymentMethodLabels)
 * @param keys Danh sách các keys cần loại bỏ (một hoặc nhiều)
 */
export function omitRecord<T extends object, K extends keyof T>(obj: T, keys: K[]): Omit<T, K> {
    // Tạo một bản sao nông (shallow copy) của object
    const result = { ...obj }

    // Duyệt qua danh sách keys và xóa chúng khỏi object mới
    keys.forEach((key) => {
        delete result[key]
    })

    return result as Omit<T, K>
}
