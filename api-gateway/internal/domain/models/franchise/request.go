package franchise

import (
	"api-gateway/internal/domain/models/common"
	"time"
)

type FileItem struct {
	Data        []byte
	FileName    string
	ContentType string
}

// [1] Đối tác gửi hồ sơ (Thường dùng multipart/form-data)
type SubmitApplicationRequest struct {
	FullName       string  `form:"fullName" json:"fullName" binding:"required"`
	Email          string  `form:"email" json:"email" binding:"required,email"`
	Phone          string  `form:"phone" json:"phone" binding:"required"`
	IdentityNumber string  `form:"identityNumber" json:"identityNumber" binding:"required"`
	ProvinceId     string  `form:"provinceId" json:"provinceId" binding:"required"`
	DistrictId     string  `form:"districtId" json:"districtId" binding:"required"`
	WardId         string  `form:"wardId" json:"wardId" binding:"required"`
	StoreName      string  `form:"storeName" json:"storeName" binding:"required"`
	StoreAddress   string  `form:"storeAddress" json:"storeAddress" binding:"required"`
	StoreLatitude  float64 `form:"storeLatitude" json:"storeLatitude"`
	StoreLongitude float64 `form:"storeLongitude" json:"storeLongitude"`
	StoreSizeSqm   float64 `form:"storeSizeSqm" json:"storeSizeSqm"`
	// Lưu ý: Files (LandCertificates, StoreFront) xử lý riêng qua c.Request.MultipartForm
}

// [3] Admin lấy danh sách hồ sơ (Filter + Pagination)
type GetAllApplicationsRequest struct {
	common.PaginationQuery        // Nhúng page, pageSize
	Status                 *int32 `form:"status" json:"status"`
	SearchKey              string `form:"searchKey" json:"searchKey"`
}

// [4] Admin duyệt hồ sơ
type ReviewFranchiseApplicationRequest struct {
	Action                 int32     `json:"action" binding:"required"` // 1: Approve, 2: Reject
	Note                   string    `json:"note"`
	DepositAmount          float64   `json:"depositAmount"`
	FranchiseFee           float64   `json:"franchiseFee"`
	RevenueSharePercentage float64   `json:"revenueSharePercentage"`
	ValidFrom              time.Time `json:"validFrom"`
	ValidUntil             time.Time `json:"validUntil"`
}

// [5] Lấy danh sách hợp đồng (Filter + Pagination)
type GetAllContractsRequest struct {
	common.PaginationQuery
	Status *int32 `form:"status" json:"status"`
}

// [7] Xác nhận thanh toán cọc
type ConfirmDepositPaymentRequest struct {
	Note string `json:"note"`
}

// [8] Cập nhật tiến độ thi công
type UpdateSetupStatusRequest struct {
	SetupStatus int32 `json:"setupStatus"` // 0: Design, 1: Construction, 2: Inspection, 3: Ready
}

// [9] Cập nhật trạng thái hợp đồng (Suspend/Terminate)
type UpdateContractStatusRequest struct {
	Status int32  `json:"status" binding:"required"`
	Reason string `json:"reason" binding:"required"`
}

// [10] Gia hạn hợp đồng
type RenewContractRequest struct {
	ExtensionYears int32  `json:"extensionYears" binding:"required"`
	Note           string `json:"note"`
}
