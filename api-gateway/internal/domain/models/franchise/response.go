package franchise

import (
	"api-gateway/internal/domain/models/common"
	"time"
)

// Response cho Application
type FranchiseApplicationResponse struct {
	Id                  string                      `json:"id"`
	StoreId             string                      `json:"storeId"`
	ManagerId           string                      `json:"managerId"`
	FullName            string                      `json:"fullName"`
	Email               string                      `json:"email"`
	Phone               string                      `json:"phone"`
	IdentityNumber      string                      `json:"identityNumber"`
	ProvinceId          string                      `json:"provinceId"`
	DistrictId          string                      `json:"districtId"`
	WardId              string                      `json:"wardId"`
	StoreName           string                      `json:"storeName"`
	StoreAddress        string                      `json:"storeAddress"`
	StoreLatitude       float64                     `json:"storeLatitude"`
	StoreLongitude      float64                     `json:"storeLongitude"`
	StoreSizeSqm        float64                     `json:"storeSizeSqm"`
	LandCertificateUrls []string                    `json:"landCertificateUrls"`
	StoreFrontUrl       string                      `json:"storeFrontUrl"`
	Status              int32                       `json:"status"`
	AdminNote           string                      `json:"adminNote"`
	CreatedAt           time.Time                   `json:"createdAt"`
	UpdatedAt           time.Time                   `json:"updatedAt"`
	ReviewedBy          *string                     `json:"reviewedBy,omitempty"`
	ReviewedAt          *time.Time                  `json:"reviewedAt,omitempty"`
	Contracts           []FranchiseContractResponse `json:"contracts,omitempty"`
}

// Response chi tiết hồ sơ (Thường bao gồm cả list contracts nếu có)
type FranchiseApplicationDetailResponse struct {
	Application FranchiseApplicationResponse `json:"application"`
	Contracts   []FranchiseContractResponse  `json:"contracts"`
}

// Response cho Contract
type FranchiseContractResponse struct {
	Id                     string     `json:"id"`
	ApplicationId          string     `json:"applicationId"`
	DocuSealSubmissionId   int32      `json:"docuSealSubmissionId"`
	ContractPdfUrl         string     `json:"contractPdfUrl"`
	DepositAmount          float64    `json:"depositAmount"`
	DepositStatus          int32      `json:"depositStatus"` // 0: Awaiting, 1: Paid
	FranchiseFee           float64    `json:"franchiseFee"`
	RevenueSharePercentage float64    `json:"revenueSharePercentage"`
	Status                 int32      `json:"status"`
	SetupStatus            int32      `json:"setupStatus"`
	ValidFrom              *time.Time `json:"validFrom,omitempty"`
	ValidUntil             *time.Time `json:"validUntil,omitempty"`
	CreatedAt              time.Time  `json:"createdAt"`
	UpdatedAt              time.Time  `json:"updatedAt"`
}

// Phản hồi sau khi Review (Approve/Reject)
type ReviewApplicationResponse struct {
	Message              string `json:"message"`
	ContractId           string `json:"contractId,omitempty"`
	DocuSealSubmissionId int32  `json:"docuSealSubmissionId,omitempty"`
}

// --- Dữ liệu phân trang trả về cho danh sách ---

type ApplicationPaginationResult common.PaginationResult[FranchiseApplicationResponse]
type ContractPaginationResult common.PaginationResult[FranchiseContractResponse]
