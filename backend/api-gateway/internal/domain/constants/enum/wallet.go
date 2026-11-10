package enum

// Wallet
type WalletStatus int32

const (
	WalletStatus_WALLET_STATUS_INACTIVE  WalletStatus = 1
	WalletStatus_WALLET_STATUS_ACTIVE    WalletStatus = 2
	WalletStatus_WALLET_STATUS_LOCKED    WalletStatus = 3
	WalletStatus_WALLET_STATUS_SUSPENDED WalletStatus = 4
)

type TransactionType int32

const (
	TransactionType_TRANSACTION_TYPE_DEPOSIT    TransactionType = 1
	TransactionType_TRANSACTION_TYPE_PAYMENT    TransactionType = 2
	TransactionType_TRANSACTION_TYPE_REFUND     TransactionType = 3
	TransactionType_TRANSACTION_TYPE_ADJUSTMENT TransactionType = 4
	TransactionType_TRANSACTION_TYPE_WITHDRAWAL TransactionType = 5
	TransactionType_TRANSACTION_TYPE_TRANSFER   TransactionType = 6
)

type TransactionStatus int32

const (
	TransactionStatus_TRANSACTION_STATUS_PENDING   TransactionStatus = 1
	TransactionStatus_TRANSACTION_STATUS_COMPLETED TransactionStatus = 2
	TransactionStatus_TRANSACTION_STATUS_FAILED    TransactionStatus = 3
	TransactionStatus_TRANSACTION_STATUS_CANCELLED TransactionStatus = 4
)

type WalletReferenceType int32

const (
	WalletReferenceType_REFERENCE_TYPE_ORDER         WalletReferenceType = 1
	WalletReferenceType_REFERENCE_TYPE_REFUND_TICKET WalletReferenceType = 2
	WalletReferenceType_REFERENCE_TYPE_INTERNAL_USER WalletReferenceType = 3
	WalletReferenceType_REFERENCE_TYPE_SYSTEM        WalletReferenceType = 4
)

type WalletTransactionPaymentMethod int32

const (
	WalletTransactionPaymentMethod_PAYMENT_METHOD_CASH   WalletTransactionPaymentMethod = 1
	WalletTransactionPaymentMethod_PAYMENT_METHOD_VNPAY  WalletTransactionPaymentMethod = 2
	WalletTransactionPaymentMethod_PAYMENT_METHOD_WALLET WalletTransactionPaymentMethod = 3
)

// Refund

type RefundTicketStatus int32

const (
	RefundTicketStatus_REFUND_STATUS_PENDING    RefundTicketStatus = 1
	RefundTicketStatus_REFUND_STATUS_APPROVED   RefundTicketStatus = 2
	RefundTicketStatus_REFUND_STATUS_PROCESSING RefundTicketStatus = 3
	RefundTicketStatus_REFUND_STATUS_COMPLETED  RefundTicketStatus = 4
	RefundTicketStatus_REFUND_STATUS_FAILED     RefundTicketStatus = 5
	RefundTicketStatus_REFUND_STATUS_REJECTED   RefundTicketStatus = 6
	RefundTicketStatus_REFUND_STATUS_CANCELLED  RefundTicketStatus = 7
)

type RefundReasonType int32

const (
	RefundReasonType_REFUND_REASON_NORMAL_PROCESS     RefundReasonType = 1
	RefundReasonType_REFUND_REASON_SYSTEM_ERROR       RefundReasonType = 2
	RefundReasonType_REFUND_REASON_CUSTOMER_COMPLAINT RefundReasonType = 3
	RefundReasonType_REFUND_REASON_GOODWILL_GESTURE   RefundReasonType = 4
	RefundReasonType_REFUND_REASON_COMPENSATION       RefundReasonType = 5
)

type RefundTicketMethod int32

const (
	RefundTicketMethod_REFUND_METHOD_TRANSFER RefundTicketMethod = 1
	RefundTicketMethod_REFUND_METHOD_CASH     RefundTicketMethod = 2
)

type WithdrawalStatus int32

const (
	WithdrawalStatusPending    WithdrawalStatus = 1 // Chờ duyệt
	WithdrawalStatusApproved   WithdrawalStatus = 2 // Đã duyệt
	WithdrawalStatusProcessing WithdrawalStatus = 3 // Đang xử lý
	WithdrawalStatusCompleted  WithdrawalStatus = 4 // Thành công
	WithdrawalStatusFailed     WithdrawalStatus = 5 // Thất bại
	WithdrawalStatusRejected   WithdrawalStatus = 6 // Từ chối
	WithdrawalStatusCancelled  WithdrawalStatus = 7 // Đã hủy
)
