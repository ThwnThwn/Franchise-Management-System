package engagement

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/engagement"
	"api-gateway/internal/domain/models/loyalty"
	"api-gateway/pkg/utils/validation"
	"net/http"
	"strconv"

	"github.com/gin-gonic/gin"
)

type LoyaltyHandler struct {
	loyaltyGW engagement.ILoyaltyAccountService
}

func NewLoyaltyHandler(loyaltyGW engagement.ILoyaltyAccountService) *LoyaltyHandler {
	return &LoyaltyHandler{
		loyaltyGW: loyaltyGW,
	}
}

// GetLoyaltyAccounts
// @Summary      Search Loyalty Accounts
// @Description  Retrieve loyalty accounts by user phone or email.
// @Tags         Loyalty
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        query  query     loyalty.GetByUserPhoneOrEmailRequest  true  "Search criteria (phone or email)"
// @Success      200    {array}   loyalty.LoyaltyAccountResponse "List of loyalty accounts"
// @Failure      400    {object}  map[string]string              "Validation error"
// @Failure      401    {object}  map[string]string              "Unauthorized"
// @Router       /loyalty/accounts [get]
func (h *LoyaltyHandler) GetLoyaltyAccounts(c *gin.Context) error {
	var req loyalty.GetByUserPhoneOrEmailRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	res, err := h.loyaltyGW.GetByUserPhoneOrEmail(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetLoyaltyReport
// @Summary      Get Loyalty Report Summary (JSON)
// @Description  Get statistics like total members, points, and tier distribution for dashboard.
// @Tags         Loyalty
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        query  query     loyalty.GetLoyaltyReportRequest  false  "Filter params (fromDate, toDate, tierId)"
// @Success      200    {object}  loyalty.LoyaltyReportResponse     "Loyalty summary data"
// @Router       /loyalty/reports [get]
func (h *LoyaltyHandler) GetLoyaltyReport(c *gin.Context) error {
	var req loyalty.GetLoyaltyReportRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	// Ép cứng format là VIEW (0)
	req.Format = 0

	// Nhận 3 tham số từ Service cập nhật
	data, _, err := h.loyaltyGW.GetLoyaltyReport(ctx, &req)
	if err != nil {
		return err
	}

	// Check nil để tránh crash hoặc trả về data rỗng không mong muốn
	if data == nil {
		return errorcode.NewInternalError("Không có dữ liệu báo cáo")
	}

	c.JSON(http.StatusOK, data)
	return nil
}

// ExportLoyaltyReport
// @Summary      Export Loyalty Report (PDF/CSV)
// @Description  Download loyalty statistics as a PDF or CSV file.
// @Tags         Loyalty
// @Produce      application/octet-stream
// @Security     BearerAuth
// @Param        query   query     loyalty.GetLoyaltyReportRequest  false  "Filter params"
// @Param        format  query     int                             true   "1 for PDF, 2 for CSV"
// @Success      200     {file}    binary                          "Report file"
// @Router       /loyalty/reports/export [get]
func (h *LoyaltyHandler) ExportLoyaltyReport(c *gin.Context) error {
	var req loyalty.GetLoyaltyReportRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	// 1. Chỉ kiểm tra khác 0 và phải là số hợp lệ
	formatStr := c.Query("format")
	format, err := strconv.Atoi(formatStr)
	if err != nil || format == 0 {
		return errorcode.NewBadRequest("Định dạng xuất file không hợp lệ")
	}
	req.Format = int32(format)

	ctx := c.Request.Context()

	// 2. Gọi Service, nhận 3 tham số
	_, export, err := h.loyaltyGW.GetLoyaltyReport(ctx, &req)
	if err != nil {
		return err
	}

	// 3. Check nil cho struct export và nội dung file
	if export == nil || len(export.FileContent) == 0 {
		return errorcode.NewInternalError("Không thể tạo file báo cáo hoặc dữ liệu trống")
	}

	// 4. Trả về file buffer
	c.Header("Content-Disposition", "attachment; filename="+export.FileName)
	c.Data(http.StatusOK, "application/octet-stream", export.FileContent)
	return nil
}
