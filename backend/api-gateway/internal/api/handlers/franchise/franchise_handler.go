package franchise

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/franchise"
	franchiseModel "api-gateway/internal/domain/models/franchise"
	"api-gateway/pkg/utils/contextutils"
	"api-gateway/pkg/utils/validation"
	"encoding/json"
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
// @Summary      Submit franchise application
// @Description  Partner submits application (personal info, premise address) along with storefront and land certificate images.
// @Tags         Franchise: Applications
// @Security     BearerAuth
// @Accept       multipart/form-data
// @Produce      json
// @Param        fullName        formData string  true  "Full name"
// @Param        email           formData string  true  "Contact email"
// @Param        phone           formData string  true  "Phone number"
// @Param        identityNumber  formData string  true  "Identity card number (CMND/CCCD)"
// @Param        provinceId      formData string  true  "Province/City ID"
// @Param        districtId      formData string  true  "District ID"
// @Param        wardId          formData string  true  "Ward ID"
// @Param        storeName       formData string  true  "Proposed store name"
// @Param        storeAddress    formData string  true  "Detailed premise address"
// @Param        storeLatitude   formData number  false "Latitude"
// @Param        storeLongitude  formData number  false "Longitude"
// @Param        storeSizeSqm    formData number  false "Premise area (sqm)"
// @Param        storeFront      formData file    false "Storefront image (1 file)"
// @Param        landCerts       formData file    false "Land certificate images (Multiple files)"
// @Success      201  {object}  franchiseModel.FranchiseApplicationResponse "Created application"
// @Failure      400  {object}  object "Validation error (Bad Request)"
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
// @Summary      Get all franchise applications (Admin)
// @Description  Retrieve all franchise applications (supports pagination, filtering by status).
// @Tags         Franchise: Applications
// @Security     BearerAuth
// @Produce      json
// @Param        query  query  franchiseModel.GetAllApplicationsRequest false "Pagination and filters"
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
// @Summary      Get my franchise applications
// @Description  Retrieve all franchise applications submitted by the current user.
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
// @Summary      Get application details (Admin)
// @Description  View application details including the list of contracts generated from this application.
// @Tags         Franchise: Applications
// @Security     BearerAuth
// @Produce      json
// @Param        id  path  string true "Application ID"
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
// @Summary      Review franchise application (Admin)
// @Description  Admin approves or rejects the application. If approved, the system automatically generates a Contract and sends a DocuSeal link.
// @Tags         Franchise: Applications
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id    path  string true "Application ID"
// @Param        body  body  franchiseModel.ReviewFranchiseApplicationRequest true "Review decision & contract parameters"
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
// @Summary      Get all contracts (Admin/Accountant)
// @Description  Retrieve all franchise contracts (supports pagination, filtering by status).
// @Tags         Franchise: Contracts
// @Security     BearerAuth
// @Produce      json
// @Param        query  query  franchiseModel.GetAllContractsRequest false "Pagination and filters"
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
// @Summary      Get my contracts
// @Description  Retrieve all franchise contracts of the current user.
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
// @Summary      Get contract details
// @Description  Retrieve detailed information of a franchise contract.
// @Tags         Franchise: Contracts
// @Security     BearerAuth
// @Produce      json
// @Param        id  path  string true "Contract ID"
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
// @Summary      Confirm deposit payment (Accountant)
// @Description  Change contract status to Active, create a new Store, and grant Manager role to the partner.
// @Tags         Franchise: Contracts
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id    path  string true "Contract ID"
// @Param        body  body  franchiseModel.ConfirmDepositPaymentRequest true "Transaction notes"
// @Success      200   {object} map[string]string "{"message": "Xác nhận thanh toán thành công. Hợp đồng đã có hiệu lực."}"
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
// @Summary      Update store setup status
// @Description  Admin updates the construction status (Design -> Construction -> Inspection -> Ready).
// @Tags         Franchise: Contracts
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id    path  string true "Contract ID"
// @Param        body  body  franchiseModel.UpdateSetupStatusRequest true "Setup status"
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
// @Summary      Suspend / Terminate contract
// @Description  Admin changes the contract status (Suspended, Terminated) with a reason.
// @Tags         Franchise: Contracts
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id    path  string true "Contract ID"
// @Param        body  body  franchiseModel.UpdateContractStatusRequest true "Status and reason"
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
// @Summary      Renew franchise contract
// @Description  Add valid years to the current contract.
// @Tags         Franchise: Contracts
// @Security     BearerAuth
// @Accept       json
// @Produce      json
// @Param        id    path  string true "Contract ID"
// @Param        body  body  franchiseModel.RenewContractRequest true "Years to renew"
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
// @Summary      Handle DocuSeal Webhook (Internal)
// @Description  Update contract status (Signed) based on the webhook sent from DocuSeal. Uses HMAC Header instead of Token authentication.
// @Tags         Franchise: Webhooks
// @Accept       json
// @Produce      json
// @Success      200 {object} map[string]string "{"message": "Webhook processed"}"
// @Router       /franchise/webhook/docuseal [post]
func (h *FranchiseHandler) HandleDocuSealWebhook(c *gin.Context) error {
	// 1. Lấy chữ ký từ header
	signature := c.GetHeader("X-Docuseal-Signature")

	// 2. Đọc nguyên bản body (RAW) để giữ nguyên cấu trúc tính HMAC cho C#
	bodyBytes, err := io.ReadAll(c.Request.Body)
	if err != nil {
		return errorcode.NewBadRequest("Không thể đọc dữ liệu webhook")
	}
	rawBody := string(bodyBytes)

	// 3. Parse JSON vào struct để trích xuất các field cần thiết
	var payload franchiseModel.DocuSealPayload
	if err := json.Unmarshal(bodyBytes, &payload); err != nil {
		// Trả về lỗi nếu DocuSeal gửi JSON tào lao
		return errorcode.NewBadRequest("Dữ liệu webhook không hợp lệ")
	}

	// 4. Đẩy toàn bộ thông tin xuống Client
	ctx := c.Request.Context()
	if err := h.franchiseGW.HandleDocuSealWebhook(ctx, signature, rawBody, &payload); err != nil {
		return err // Trả về lỗi để DocuSeal thử lại (Retry)
	}

	c.JSON(http.StatusOK, gin.H{"message": "Webhook processed"})
	return nil
}
