export function maskEmail(email: string) {
    const [username, domain] = email.split("@")
    if (username.length <= 2) {
        return username[0] + "***@" + domain
    }
    return username.slice(0, 2) + "***@" + domain
}

/**
 * Format chuỗi số điện thoại sang dạng xxxx.xxx.xxx
 * @param phone Chuỗi số điện thoại đầu vào (VD: "0123234234", "0123 234 234")
 * @returns Chuỗi đã format hoặc chuỗi rỗng nếu không hợp lệ
 */
export const formatPhoneNumber = (phone: string | undefined | null): string => {
    if (!phone) return ""

    // 1. Loại bỏ tất cả ký tự không phải số
    const cleaned = phone.replace(/\D/g, "")

    // 2. Kiểm tra độ dài (thường là 10 số tại VN)
    // Logic: Chặt chuỗi thành 3 phần: 4 số đầu - 3 số giữa - các số còn lại
    const match = cleaned.match(/^(\d{4})(\d{3})(\d{3,4})$/)

    if (match) {
        return `${match[1]}.${match[2]}.${match[3]}`
    }

    return cleaned // Trả về chuỗi chỉ có số nếu không khớp format 10-11 số
}
