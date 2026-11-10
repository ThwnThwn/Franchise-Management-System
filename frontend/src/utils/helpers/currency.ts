/**
 * Hàm chuẩn hóa input từ DB: Đổi "50000,00" hoặc "50000.00" về số chuẩn
 */
export const normalizeCurrency = (n: number | string | null | undefined): number => {
    if (n === null || n === undefined || n === "") return 0
    if (typeof n === "number") return n

    // Nếu là string: Đổi phẩy thành chấm, sau đó parse
    const normalized = n.toString().replace(",", ".")
    const parsed = parseFloat(normalized)

    return isNaN(parsed) ? 0 : parsed
}

export const formatCurrency = (n: number | string | null | undefined) => {
    const val = Math.abs(normalizeCurrency(n)) // Lấy giá trị tuyệt đối

    return new Intl.NumberFormat("vi-VN", {
        minimumFractionDigits: 0,
        maximumFractionDigits: 0 // CHỐT: Vứt bỏ hoàn toàn số lẻ
    }).format(val)
}

export const formatCurrencyWithSymbol = (n: number | string | null | undefined) => {
    const val = Math.abs(normalizeCurrency(n))

    return new Intl.NumberFormat("vi-VN", {
        style: "currency",
        currency: "VND",
        minimumFractionDigits: 0,
        maximumFractionDigits: 0 // CHỐT: Vứt bỏ hoàn toàn số lẻ
    }).format(val)
}
