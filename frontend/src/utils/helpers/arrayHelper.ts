/**
 * Helper để chuyển đổi bất kỳ Enum nào thành mảng các giá trị (values)
 * @param enumObj Đối tượng Enum cần chuyển đổi
 */
export const enumToValues = <T extends Record<string, any>>(enumObj: T): T[keyof T][] => {
    return Object.values(enumObj).filter(
        (value) => typeof value === "number" || !Object.keys(enumObj).includes(value.toString())
    ) as T[keyof T][]
}
