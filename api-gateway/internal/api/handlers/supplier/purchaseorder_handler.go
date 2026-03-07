package supplier

import (
	"api-gateway/internal/application/constants/errorcode"
	gw "api-gateway/internal/application/gateways/supplier"
	_ "api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/supplier"
	"api-gateway/pkg/utils/grpcutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type PurchaseOrderHandler struct {
	poGW gw.IPurchaseOrderService
}

func NewPurchaseOrderHandler(
	poGW gw.IPurchaseOrderService,
) *PurchaseOrderHandler {
	return &PurchaseOrderHandler{
		poGW: poGW,
	}
}

// GetAll
// @Summary      Danh sách đơn nhập hàng
// @Description  Lấy danh sách đơn nhập hàng có phân trang và lọc theo Supplier, Store hoặc Status.
// @Tags         Suppliers / PurchaseOrders
// @Produce      json
// @Security     BearerAuth
// @Param        query query supplier.GetPurchaseOrdersReq true "Tham số lọc và phân trang"
// @Success      200 {object} common.PaginationResult[supplier.PurchaseOrderSummary]
// @Router       /suppliers/purchase-orders [get]
func (h *PurchaseOrderHandler) GetAll(c *gin.Context) error {
	var req supplier.GetPurchaseOrdersReq
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	res, err := h.poGW.GetAll(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetById
// @Summary      Chi tiết đơn nhập hàng
// @Description  Lấy thông tin chi tiết của một đơn nhập hàng kèm danh sách nguyên liệu bên trong.
// @Tags         Suppliers / PurchaseOrders
// @Produce      json
// @Security     BearerAuth
// @Param        id path string true "UUID của đơn nhập hàng"
// @Success      200 {object} supplier.PurchaseOrderDetail
// @Router       /suppliers/purchase-orders/{id} [get]
func (h *PurchaseOrderHandler) GetById(c *gin.Context) error {
	idStr := c.Param("id")
	id, err := uuid.Parse(idStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	res, err := h.poGW.GetById(ctx, id)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// Create
// @Summary      Tạo đơn nhập hàng mới
// @Description  Tạo một bản ghi đơn nhập hàng mới ở trạng thái PENDING.
// @Tags         Suppliers / PurchaseOrders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body supplier.CreatePurchaseOrderReq true "Thông tin đơn nhập hàng"
// @Success      200 {object} supplier.PurchaseOrderBaseRes
// @Router       /suppliers/purchase-orders [post]
func (h *PurchaseOrderHandler) Create(c *gin.Context) error {
	var req supplier.CreatePurchaseOrderReq
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	res, err := h.poGW.Create(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// Update
// @Summary      Cập nhật đơn nhập hàng
// @Description  Chỉnh sửa thông tin đơn nhập hàng (chỉ thực hiện được khi đơn ở trạng thái cho phép).
// @Tags         Suppliers / PurchaseOrders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id path string true "UUID của đơn nhập hàng"
// @Param        request body supplier.UpdatePurchaseOrderReq true "Thông tin cập nhật"
// @Success      200 {object} supplier.PurchaseOrderBaseRes
// @Router       /suppliers/purchase-orders/{id} [patch]
func (h *PurchaseOrderHandler) Update(c *gin.Context) error {
	idStr := c.Param("id")
	id, err := uuid.Parse(idStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req supplier.UpdatePurchaseOrderReq
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	res, err := h.poGW.Update(ctx, id, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// Cancel
// @Summary      Hủy đơn nhập hàng
// @Description  Hủy bỏ đơn nhập hàng và ghi rõ lý do hủy.
// @Tags         Suppliers / PurchaseOrders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id path string true "UUID của đơn nhập hàng"
// @Param        request body supplier.CancelPurchaseOrderReq true "Lý do hủy"
// @Success      200 {object} supplier.PurchaseOrderBaseRes
// @Router       /suppliers/purchase-orders/{id}/cancel [patch]
func (h *PurchaseOrderHandler) Cancel(c *gin.Context) error {
	idStr := c.Param("id")
	id, err := uuid.Parse(idStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req supplier.CancelPurchaseOrderReq
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	res, err := h.poGW.Cancel(ctx, id, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// Complete
// @Summary      Hoàn tất nhập hàng
// @Description  Xác nhận đã nhận đủ hàng và chốt đơn nhập hàng (thường dùng để tăng tồn kho).
// @Tags         Suppliers / PurchaseOrders
// @Produce      json
// @Security     BearerAuth
// @Param        id path string true "UUID của đơn nhập hàng"
// @Success      200 {object} supplier.PurchaseOrderBaseRes
// @Router       /suppliers/purchase-orders/{id}/complete [patch]
func (h *PurchaseOrderHandler) Complete(c *gin.Context) error {
	idStr := c.Param("id")
	id, err := uuid.Parse(idStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	res, err := h.poGW.Complete(ctx, id)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetLatestPrices
// @Summary      Giá nhập mới nhất của nguyên liệu
// @Description  Tra cứu giá của danh sách ID nguyên liệu dựa trên các hóa đơn nhập gần nhất.
// @Tags         Suppliers / PurchaseOrders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body supplier.GetLatestPricesReq true "Danh sách ID nguyên liệu"
// @Success      200 {object} map[string]float64
// @Router       /suppliers/purchase-orders/latest-prices [post]
func (h *PurchaseOrderHandler) GetLatestPrices(c *gin.Context) error {
	var req supplier.GetLatestPricesReq
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	res, err := h.poGW.GetLatestPrices(ctx, req.IngredientIds)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}
