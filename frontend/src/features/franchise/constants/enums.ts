/** Trạng thái hồ sơ đăng ký nhượng quyền */
export enum FranchiseApplicationStatus {
    Pending = 0, // Chờ thẩm định
    Approved = 1, // Duyệt hồ sơ → sinh Contract
    Rejected = 2 // Từ chối
}

/** Trạng thái thanh toán tiền cọc / phí nhượng quyền đợt 1 */
export enum DepositStatus {
    AwaitingPayment = 0, // Chưa thanh toán
    Paid = 1 // Đã thanh toán
}

/** Tiến độ thi công / setup cửa hàng sau khi hợp đồng active */
export enum ContractSetupStatus {
    Design = 0, // Đang thiết kế
    Construction = 1, // Đang thi công
    Inspection = 2, // Nghiệm thu
    Ready = 3 // Sẵn sàng khai trương
}

/** Trạng thái hợp đồng nhượng quyền */
export enum ContractStatus {
    Draft = 0, // Vừa tạo, đang soạn thảo
    Approved = 1, // Giám đốc / admin đã duyệt các điều khoản
    AwaitingSignature = 2, // Đã gửi DocuSeal, chờ ký
    AwaitingPayment = 3, // Ký xong, chờ chuyển khoản tiền cọc
    Active = 4, // Đã thanh toán, hợp đồng có hiệu lực
    Suspended = 5, // Tạm ngưng (vi phạm / đang giải quyết tranh chấp)
    Terminated = 6, // Chấm dứt hợp đồng trước hạn
    Expired = 7 // Hết hạn tự nhiên
}
