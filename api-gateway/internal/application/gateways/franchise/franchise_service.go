package franchise

import (
	franchiseModel "api-gateway/internal/domain/models/franchise"
	"context"

	"github.com/google/uuid"
)

type IFranchiseService interface {
	// ── FRANCHISE APPLICATIONS ───────────────────────────────────────────────

	// SubmitApplication: Đối tác nộp hồ sơ. ManagerId lấy từ Token.
	SubmitApplication(ctx context.Context, managerId uuid.UUID,
		req *franchiseModel.SubmitApplicationRequest, storeFront *franchiseModel.FileItem, landCerts []*franchiseModel.FileItem) (
		*franchiseModel.FranchiseApplicationResponse, error,
	)

	// GetApplicationDetail: Lấy chi tiết hồ sơ kèm danh sách hợp đồng.
	GetApplicationDetail(ctx context.Context, id uuid.UUID) (*franchiseModel.FranchiseApplicationDetailResponse, error)

	// GetAllApplications: Admin lấy danh sách hồ sơ có phân trang & lọc.
	GetAllApplications(ctx context.Context, params *franchiseModel.GetAllApplicationsRequest) (
		*franchiseModel.ApplicationPaginationResult, error,
	)

	GetMyApplications(ctx context.Context) ([]*franchiseModel.FranchiseApplicationResponse, error)

	// ReviewApplication: Admin duyệt/từ chối hồ sơ.
	ReviewApplication(ctx context.Context, id uuid.UUID, req *franchiseModel.ReviewFranchiseApplicationRequest) (
		*franchiseModel.ReviewApplicationResponse, error,
	)

	// ── FRANCHISE CONTRACTS ──────────────────────────────────────────────────

	// GetAllContracts: Lấy danh sách hợp đồng (Admin/Accountant).
	GetAllContracts(ctx context.Context, params *franchiseModel.GetAllContractsRequest) (
		*franchiseModel.ContractPaginationResult, error)

	// GetContractDetail: Lấy chi tiết một hợp đồng.
	GetContractDetail(ctx context.Context, contractId uuid.UUID) (*franchiseModel.FranchiseContractResponse, error)

	GetMyContracts(ctx context.Context) ([]*franchiseModel.FranchiseContractResponse, error)

	// ConfirmDepositPayment: Kế toán xác nhận đã nhận tiền cọc.
	ConfirmDepositPayment(ctx context.Context, contractId uuid.UUID, accountantId uuid.UUID,
		req *franchiseModel.ConfirmDepositPaymentRequest) error

	// UpdateSetupStatus: Admin cập nhật tiến độ thi công (Design -> Construction...).
	UpdateSetupStatus(ctx context.Context, contractId uuid.UUID, adminId uuid.UUID,
		req *franchiseModel.UpdateSetupStatusRequest) (*franchiseModel.FranchiseContractResponse, error)

	// UpdateContractStatus: Admin tạm ngưng hoặc chấm dứt hợp đồng.
	UpdateContractStatus(ctx context.Context, contractId uuid.UUID, req *franchiseModel.UpdateContractStatusRequest) (
		*franchiseModel.FranchiseContractResponse, error)

	// RenewContract: Gia hạn hợp đồng thêm số năm.
	RenewContract(ctx context.Context, contractId uuid.UUID, req *franchiseModel.RenewContractRequest) (
		*franchiseModel.FranchiseContractResponse, error)

	// ── DOCUSEAL WEBHOOK ─────────────────────────────────────────────────────

	// HandleDocuSealWebhook: Nhận dữ liệu từ DocuSeal để cập nhật trạng thái ký kết.
	HandleDocuSealWebhook(ctx context.Context, signature string, rawBody string) error
}
