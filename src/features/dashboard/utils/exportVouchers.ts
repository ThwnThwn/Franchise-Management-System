import { Voucher, VoucherType, VoucherItemType } from "@/features/orders/models/voucher"

// ── Helpers ─────────────────────────────────────────────────────────────────

const voucherTypeLabel = (type: VoucherType): string => {
    switch (type) {
        case VoucherType.SHIPPING_FIXED_AMOUNT: return "Freeship (Tiền)"
        case VoucherType.SHIPPING_PERCENTAGE: return "Freeship (%)"
        case VoucherType.DISCOUNT_FIXED_AMOUNT: return "Ưu đãi (Tiền)"
        case VoucherType.DISCOUNT_PERCENTAGE: return "Ưu đãi (%)"
        case VoucherType.GIFT: return "Quà tặng"
        default: return String(type)
    }
}

const discountValueLabel = (v: Voucher): string => {
    if (v.type === VoucherType.GIFT) return "Theo Quà tặng"
    const suffix =
        v.type === VoucherType.SHIPPING_PERCENTAGE || v.type === VoucherType.DISCOUNT_PERCENTAGE
            ? "%" : "đ"
    return `${Number(v.discount_value).toLocaleString("vi-VN")}${suffix}`
}

/**
 * Map variant_id → tên hiển thị
 * Key = variant id, Value = "Tên SP - SKU (thuộc tính)"
 */
export type VariantNameMap = Record<string, string>

/** Build danh sách item text cho 1 voucher theo loại */
const buildItemList = (
    v: Voucher,
    type: VoucherItemType,
    variantMap: VariantNameMap
): string => {
    const items = v.voucher_items?.filter((i) => i.type === type) ?? []
    if (items.length === 0) return ""
    return items
        .map((i) => {
            const name = variantMap[i.sku] ?? i.sku
            return `${name} x${i.quantity}`
        })
        .join("; ")
}

/** Trigger browser download from a Blob */
const downloadBlob = (blob: Blob, filename: string) => {
    const url = URL.createObjectURL(blob)
    const a = document.createElement("a")
    a.href = url
    a.download = filename
    a.click()
    URL.revokeObjectURL(url)
}

// ── CSV Export ──────────────────────────────────────────────────────────────
const CSV_HEADERS = [
    "Mã Code",
    "Loại",
    "Mức giảm",
    "Giảm tối đa",
    "Đơn tối thiểu",
    "Điểm đổi",
    "SL ban đầu",
    "SL còn lại",
    "Giới hạn/người",
    "Trạng thái",
    "Ngày hết hạn",
    "Ngày tạo",
    "Quà tặng (GIFT)",
    "Item yêu cầu (REQUIRED)",
]

/** Escape a CSV field (wrap in quotes if it contains comma/newline/quote) */
const escCsv = (val: string): string => {
    if (val.includes(",") || val.includes("\n") || val.includes('"')) {
        return `"${val.replace(/"/g, '""')}"`
    }
    return val
}

/**
 * Xuất danh sách voucher ra file CSV.
 * @param vouchers  - danh sách voucher cần xuất
 * @param variantMap - map variant_id → tên hiển thị (lấy từ products)
 */
export const exportVouchersCsv = (vouchers: Voucher[], variantMap: VariantNameMap = {}) => {
    const rows = vouchers.map((v) => [
        v.code,
        voucherTypeLabel(v.type),
        discountValueLabel(v),
        v.max_discount_value ? `${Number(v.max_discount_value).toLocaleString("vi-VN")}đ` : "",
        v.min_order_value ? `${Number(v.min_order_value).toLocaleString("vi-VN")}đ` : "Mọi đơn",
        v.point_cost ? Number(v.point_cost).toLocaleString("vi-VN") : "",
        v.init_quantity?.toString() ?? "",
        v.current_quantity?.toString() ?? "",
        v.max_usage_per_user?.toString() ?? "",
        v.is_active ? "Đang hoạt động" : "Tắt",
        v.end_date ? new Date(v.end_date).toLocaleDateString("vi-VN") : "",
        v.created_at ? new Date(v.created_at).toLocaleDateString("vi-VN") : "",
        buildItemList(v, VoucherItemType.ITEM_GIFT, variantMap),
        buildItemList(v, VoucherItemType.ITEM_REQUIRED, variantMap),
    ])

    // BOM for UTF-8 Excel compatibility
    const bom = "\uFEFF"
    const csv = bom + [CSV_HEADERS, ...rows].map((r) => r.map(escCsv).join(",")).join("\r\n")

    const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" })
    downloadBlob(blob, `vouchers_${new Date().toISOString().slice(0, 10)}.csv`)
}
