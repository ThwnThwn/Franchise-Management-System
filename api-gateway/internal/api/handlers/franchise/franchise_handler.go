package franchise

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/franchise"
	franchiseModel "api-gateway/internal/domain/models/franchise"
	"api-gateway/pkg/utils/contextutils"
	"api-gateway/pkg/utils/validation"
	"io"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type FranchiseHandler struct {
	franchiseGW franchise.IFranchiseService
}

func NewFranchiseHandler(franchiseGW franchise.IFranchiseService) *FranchiseHandler {
	return &FranchiseHandler{franchiseGW: franchiseGW}
}

// ── FRANCHISE APPLICATIONS ───────────────────────────────────────────────

// SubmitApplication
// @Summary      Nộp hồ sơ đăng ký nhượng quyền
// @Description  Đối tác gửi hồ sơ (thông tin cá nhân, địa chỉ mặt bằng) kèm theo ảnh mặt bằng và các ảnh chứng nhận đất đai.
// @Tags         Franchise: Applications
// @Security     BearerAuth
// @Accept       multipart/form-data
// @Produce      json
// @Param        fullName        formData string  true  "Họ và tên"
// @Param        email           formData string  true  "Email liên hệ"
// @Param        phone           formData string  true  "Số điện thoại"
// @Param        identityNumber  formData string  true  "CMND/CCCD"
// @Param        provinceId      formData string  true  "Mã Tỉnh/Thành phố"
// @Param        districtId      formData string  true  "Mã Quận/Huyện"
// @Param        wardId          formData string  true  "Mã Phường/Xã"
// @Param        storeName       formData string  true  "Tên quán dự kiến"
// @Param        storeAddress    formData string  true  "Địa chỉ chi tiết mặt bằng"
// @Param        storeLatitude   formData number  false "Vĩ độ (Latitude)"
// @Param        storeLongitude  formData number  false "Kinh độ (Longitude)"
// @Param        storeSizeSqm    formData number  false "Diện tích mặt bằng (m2)"
// @Param        storeFront      formData file    false "Ảnh mặt tiền (1 file)"
// @Param        landCerts       formData file    false "Ảnh chứng nhận đất đai/Sổ đỏ (Nhiều file)"
// @Success      201  {object}  franchiseModel.FranchiseApplicationResponse "Hồ sơ vừa tạo"
// @Failure      400  {object}  object "Lỗi dữ liệu đầu vào"
// @Router       /franchise/applications [post]
func (h *FranchiseHandler) SubmitApplication(c *gin.Context) error {
	var req franchiseModel.SubmitApplicationRequest
	if err := c.ShouldBind(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	// 1. Xử lý ảnh StoreFront (1 file)
	var storeFrontItem *franchiseModel.FileItem
	file, err := c.FormFile("storeFront")
	if err == nil {
		f, _ := file.Open()
		defer f.Close()
		data, _ := io.ReadAll(f)
		storeFrontItem = &franchiseModel.FileItem{
			Data:        data,
			FileName:    file.Filename,
			ContentType: file.Header.Get("Content-Type"),
		}
	}

	// 2. Xử lý ảnh LandCertificates (Nhiều file)
	var landCertItems []*franchiseModel.FileItem
	form, _ := c.MultipartForm()
	if form != nil {
		files := form.File["landCerts"]
		for _, fileHeader := range files {
			f, err := fileHeader.Open()
			if err == nil {
				data, _ := io.ReadAll(f)
				landCertItems = append(landCertItems, &franchiseModel.FileItem{
					Data:        data,
					FileName:    fileHeader.Filename,
					ContentType: fileHeader.Header.Get("Content-Type"),
				})
				f.Close()
			}
		}
	}

	ctx := c.Request.Context()
	res, err := h.franchiseGW.SubmitApplication(ctx, userId, &req, storeFrontItem, landCertItems)
	if err != nil {
		return err
	}

	c.JSON(http.StatusCreated, res)
	return nil
}

// GetAllApplications
// @Summary      Lấy danh sách hồ sơ nhượng quyền (Admin)
// @Description  Lấy toàn bộ hồ sơ đăng ký nhượng quyền (hỗ trợ phân trang, lọc theo trạng thái).
// @Tags         Franchise: Applications
// @Security     BearerAuth
// @Produce      json
// @Param        query  query  franchiseModel.GetAllApplicationsRequest false "Bộ lọc và phân trang"
// @Success      200    {object} franchiseModel.ApplicationPaginationResult
// @Router       /franchise/applications [get]
func (h *FranchiseHandler) GetAllApplications(c *gin.Context) error {
	var req franchiseModel.GetAllApplicationsRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	res, err := h.franchiseGW.GetAllApplications(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetMyApplications
// @Summary      Lấy danh sách hồ sơ nhượng quyền của bản thân
// @Description  Lấy toàn bộ hồ sơ đăng ký nhượng quyền của bản thân
// @Tags         Franchise: Applications
// @Security     BearerAuth
// @Produce      json
// @Success      200    {array} franchiseModel.FranchiseApplicationResponse
// @Router       /franchise/me/applications [get]
func (h *FranchiseHandler) GetMyApplications(c *gin.Context) error {
	ctx := c.Request.Context()
	res, err := h.franchiseGW.GetMyApplications(ctx)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetApplicationDetail
// @Summary      Xem chi tiết một hồ sơ (Admin)
// @Description  Xem chi tiết hồ sơ bao gồm cả danh sách các hợp đồng (Contracts) được sinh ra từ hồ sơ này.
// @Tags         Franchise: Applications
// @Security     BearerAuth
// @Produce      json
// @Param        id  path  string true "ID Hồ sơ (Application ID)"
// @Success      200 {object} franchiseModel.FranchiseApplicationDetailResponse
// @Router       /franchise/applications/{id} [get]
func (h *FranchiseHandler) GetApplicationDetail(c *gin.Context) error {
	id, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()
	res, err := h.franchiseGW.GetApplicationDetail(ctx, id)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// ReviewApplication
// @Summary      Thẩm định hồ sơ nhượng quyền (Admin)
// @Description  Admin duyệt (Approve) hoặc từ chối (Reject) hồ sơ. Nếu Approve, hệ thống tự động sinh Hợp đồng và bắn link DocuSeal.
// @Tags         Franchise: Applications
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id    path  string true "ID Hồ sơ (Application ID)"
// @Param        body  body  franchiseModel.ReviewFranchiseApplicationRequest true "Quyết định thẩm định & Thông số hợp đồng"
// @Success      200   {object} franchiseModel.ReviewApplicationResponse
// @Router       /franchise/applications/{id}/review [patch]
func (h *FranchiseHandler) ReviewApplication(c *gin.Context) error {
	id, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req franchiseModel.ReviewFranchiseApplicationRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	res, err := h.franchiseGW.ReviewApplication(ctx, id, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// ── FRANCHISE CONTRACTS ──────────────────────────────────────────────────

// GetAllContracts
// @Summary      Lấy danh sách hợp đồng (Admin/Kế toán)
// @Description  Lấy toàn bộ hợp đồng nhượng quyền (hỗ trợ phân trang, lọc theo trạng thái).
// @Tags         Franchise: Contracts
// @Security     BearerAuth
// @Produce      json
// @Param        query  query  franchiseModel.GetAllContractsRequest false "Bộ lọc và phân trang"
// @Success      200    {object} franchiseModel.ContractPaginationResult
// @Router       /franchise/contracts [get]
func (h *FranchiseHandler) GetAllContracts(c *gin.Context) error {
	var req franchiseModel.GetAllContractsRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	res, err := h.franchiseGW.GetAllContracts(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetMyContracts
// @Summary      Lấy danh sách hợp đồng của bản thân
// @Description  Lấy toàn bộ hợp đồng nhượng quyền của bản thân
// @Tags         Franchise: Contracts
// @Security     BearerAuth
// @Produce      json
// @Success      200    {array} franchiseModel.FranchiseContractResponse
// @Router       /franchise/me/contracts [get]
func (h *FranchiseHandler) GetMyContracts(c *gin.Context) error {
	ctx := c.Request.Context()
	res, err := h.franchiseGW.GetMyContracts(ctx)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetContractDetail
// @Summary      Xem chi tiết một hợp đồng
// @Description  Lấy thông tin chi tiết của một hợp đồng nhượng quyền.
// @Tags         Franchise: Contracts
// @Security     BearerAuth
// @Produce      json
// @Param        id  path  string true "ID Hợp đồng"
// @Success      200 {object} franchiseModel.FranchiseContractResponse
// @Router       /franchise/contracts/{id} [get]
func (h *FranchiseHandler) GetContractDetail(c *gin.Context) error {
	id, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()
	res, err := h.franchiseGW.GetContractDetail(ctx, id)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// ConfirmDepositPayment
// @Summary      Xác nhận đã đóng tiền cọc (Kế toán)
// @Description  Chuyển trạng thái hợp đồng sang Active, tạo Cửa hàng mới và cấp quyền Manager cho đối tác.
// @Tags         Franchise: Contracts
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id    path  string true "ID Hợp đồng"
// @Param        body  body  franchiseModel.ConfirmDepositPaymentRequest true "Ghi chú giao dịch"
// @Success      200   {object} map[string]string "{"message": "Xác nhận thanh toán thành công"}"
// @Router       /franchise/contracts/{id}/payment [post]
func (h *FranchiseHandler) ConfirmDepositPayment(c *gin.Context) error {
	contractId, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req franchiseModel.ConfirmDepositPaymentRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	accountantId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	ctx := c.Request.Context()
	if err := h.franchiseGW.ConfirmDepositPayment(ctx, contractId, accountantId, &req); err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{"message": "Xác nhận thanh toán thành công. Hợp đồng đã có hiệu lực."})
	return nil
}

// UpdateSetupStatus
// @Summary      Cập nhật tiến độ thi công cửa hàng
// @Description  Admin cập nhật trạng thái thi công (Design -> Construction -> Inspection -> Ready).
// @Tags         Franchise: Contracts
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id    path  string true "ID Hợp đồng"
// @Param        body  body  franchiseModel.UpdateSetupStatusRequest true "Trạng thái Setup"
// @Success      200   {object} franchiseModel.FranchiseContractResponse
// @Router       /franchise/contracts/{id}/setup [patch]
func (h *FranchiseHandler) UpdateSetupStatus(c *gin.Context) error {
	contractId, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req franchiseModel.UpdateSetupStatusRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	adminId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	ctx := c.Request.Context()
	res, err := h.franchiseGW.UpdateSetupStatus(ctx, contractId, adminId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// UpdateContractStatus
// @Summary      Tạm ngưng / Chấm dứt hợp đồng
// @Description  Admin thay đổi trạng thái hợp đồng (Suspended, Terminated) kèm lý do.
// @Tags         Franchise: Contracts
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id    path  string true "ID Hợp đồng"
// @Param        body  body  franchiseModel.UpdateContractStatusRequest true "Trạng thái và lý do"
// @Success      200   {object} franchiseModel.FranchiseContractResponse
// @Router       /franchise/contracts/{id}/status [patch]
func (h *FranchiseHandler) UpdateContractStatus(c *gin.Context) error {
	contractId, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req franchiseModel.UpdateContractStatusRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	res, err := h.franchiseGW.UpdateContractStatus(ctx, contractId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// RenewContract
// @Summary      Gia hạn hợp đồng nhượng quyền
// @Description  Cộng thêm số năm hiệu lực vào hợp đồng hiện tại.
// @Tags         Franchise: Contracts
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id    path  string true "ID Hợp đồng"
// @Param        body  body  franchiseModel.RenewContractRequest true "Số năm gia hạn"
// @Success      200   {object} franchiseModel.FranchiseContractResponse
// @Router       /franchise/contracts/{id}/renew [post]
func (h *FranchiseHandler) RenewContract(c *gin.Context) error {
	contractId, err := uuid.Parse(c.Param("id"))
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req franchiseModel.RenewContractRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	res, err := h.franchiseGW.RenewContract(ctx, contractId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// ── DOCUSEAL WEBHOOK ─────────────────────────────────────────────────────

// HandleDocuSealWebhook
// @Summary      Nhận Webhook từ DocuSeal (Internal)
// @Description  Cập nhật trạng thái hợp đồng (Đã ký) dựa trên webhook từ hệ thống DocuSeal gửi về. Không dùng xác thực Token mà dùng Header HMAC.
// @Tags         Franchise: Webhooks
// @Accept       json
// @Produce      json
// @Success      200 {object} map[string]string "{"message": "Webhook processed"}"
// @Router       /franchise/webhook/docuseal [post]
func (h *FranchiseHandler) HandleDocuSealWebhook(c *gin.Context) error {
	// Lấy chữ ký từ header
	signature := c.GetHeader("X-Docuseal-Signature")

	// Đọc nguyên bản body (Không bind JSON để giữ nguyên cấu trúc tính HMAC)
	bodyBytes, err := io.ReadAll(c.Request.Body)
	if err != nil {
		return errorcode.NewBadRequest("Không thể đọc dữ liệu webhook")
	}

	ctx := c.Request.Context()
	if err := h.franchiseGW.HandleDocuSealWebhook(ctx, signature, string(bodyBytes)); err != nil {
		return err // Trả về lỗi để DocuSeal thử lại (Retry)
	}

	c.JSON(http.StatusOK, gin.H{"message": "Webhook processed"})
	return nil
}
