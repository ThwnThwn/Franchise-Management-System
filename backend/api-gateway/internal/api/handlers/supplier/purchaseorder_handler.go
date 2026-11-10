package supplier

import (
	"api-gateway/internal/application/constants/errorcode"
	gw "api-gateway/internal/application/gateways/supplier"
	_ "api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/supplier"
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
// @Summary      Get all purchase orders
// @Description  Retrieve a paginated list of purchase orders. Supports filtering by Supplier, Store, or Status.
// @Tags         Suppliers / PurchaseOrders
// @Produce      json
// @Security     BearerAuth
// @Param        query query  supplier.GetPurchaseOrdersReq true "Filtering and pagination parameters"
// @Success      200   {object} common.PaginationResult[supplier.PurchaseOrderSummary] "List of purchase orders"
// @Failure      400   {object} object "Validation Error (Bad Request)"
// @Failure      401   {object} object "Unauthorized"
// @Failure      500   {object} object "Internal Server Error"
// @Router       /suppliers/purchase-orders [get]
func (h *PurchaseOrderHandler) GetAll(c *gin.Context) error {
	var req supplier.GetPurchaseOrdersReq
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	res, err := h.poGW.GetAll(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetById
// @Summary      Get purchase order details
// @Description  Retrieve detailed information of a specific purchase order, including the list of imported ingredients.
// @Tags         Suppliers / PurchaseOrders
// @Produce      json
// @Security     BearerAuth
// @Param        id    path     string true "Purchase Order UUID"
// @Success      200   {object} supplier.PurchaseOrderDetail "Detailed purchase order information"
// @Failure      400   {object} object "Invalid UUID format"
// @Failure      401   {object} object "Unauthorized"
// @Failure      404   {object} object "Purchase Order Not Found"
// @Failure      500   {object} object "Internal Server Error"
// @Router       /suppliers/purchase-orders/{id} [get]
func (h *PurchaseOrderHandler) GetById(c *gin.Context) error {
	idStr := c.Param("id")
	id, err := uuid.Parse(idStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()

	res, err := h.poGW.GetById(ctx, id)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// Create
// @Summary      Create a new purchase order
// @Description  Create a new purchase order record with an initial status of PENDING.
// @Tags         Suppliers / PurchaseOrders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body     supplier.CreatePurchaseOrderReq true "Purchase order information"
// @Success      200     {object} supplier.PurchaseOrderBaseRes "Successfully created"
// @Failure      400     {object} object "Validation Error (Bad Request)"
// @Failure      401     {object} object "Unauthorized"
// @Failure      500     {object} object "Internal Server Error"
// @Router       /suppliers/purchase-orders [post]
func (h *PurchaseOrderHandler) Create(c *gin.Context) error {
	var req supplier.CreatePurchaseOrderReq
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	res, err := h.poGW.Create(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// Update
// @Summary      Update a purchase order
// @Description  Modify the details of an existing purchase order (only allowed when the PO is in an updatable status).
// @Tags         Suppliers / PurchaseOrders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id      path     string true "Purchase Order UUID"
// @Param        request body     supplier.UpdatePurchaseOrderReq true "Updated information"
// @Success      200     {object} supplier.PurchaseOrderBaseRes "Successfully updated"
// @Failure      400     {object} object "Validation Error or Invalid ID"
// @Failure      401     {object} object "Unauthorized"
// @Failure      404     {object} object "Purchase Order Not Found"
// @Failure      500     {object} object "Internal Server Error"
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

	ctx := c.Request.Context()

	res, err := h.poGW.Update(ctx, id, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// Cancel
// @Summary      Cancel a purchase order
// @Description  Cancel an active purchase order and record the specific cancellation reason.
// @Tags         Suppliers / PurchaseOrders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id      path     string true "Purchase Order UUID"
// @Param        request body     supplier.CancelPurchaseOrderReq true "Cancellation reason"
// @Success      200     {object} supplier.PurchaseOrderBaseRes "Successfully cancelled"
// @Failure      400     {object} object "Validation Error or Invalid ID"
// @Failure      401     {object} object "Unauthorized"
// @Failure      404     {object} object "Purchase Order Not Found"
// @Failure      500     {object} object "Internal Server Error"
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

	ctx := c.Request.Context()

	res, err := h.poGW.Cancel(ctx, id, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// Complete
// @Summary      Complete a purchase order
// @Description  Confirm that all goods have been successfully received and finalize the purchase order (typically triggers an inventory restock).
// @Tags         Suppliers / PurchaseOrders
// @Produce      json
// @Security     BearerAuth
// @Param        id      path     string true "Purchase Order UUID"
// @Success      200     {object} supplier.PurchaseOrderBaseRes "Successfully completed"
// @Failure      400     {object} object "Invalid ID format"
// @Failure      401     {object} object "Unauthorized"
// @Failure      404     {object} object "Purchase Order Not Found"
// @Failure      500     {object} object "Internal Server Error"
// @Router       /suppliers/purchase-orders/{id}/complete [patch]
func (h *PurchaseOrderHandler) Complete(c *gin.Context) error {
	idStr := c.Param("id")
	id, err := uuid.Parse(idStr)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()

	res, err := h.poGW.Complete(ctx, id)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetLatestPrices
// @Summary      Get latest ingredient prices
// @Description  Look up the most recent import prices for a provided list of ingredient IDs based on the latest purchase order records.
// @Tags         Suppliers / PurchaseOrders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body     supplier.GetLatestPricesReq true "List of ingredient IDs"
// @Success      200     {object} map[string]float64 "Map of ingredient IDs to their latest prices"
// @Failure      400     {object} object "Validation Error (Bad Request)"
// @Failure      401     {object} object "Unauthorized"
// @Failure      500     {object} object "Internal Server Error"
// @Router       /suppliers/purchase-orders/latest-prices [post]
func (h *PurchaseOrderHandler) GetLatestPrices(c *gin.Context) error {
	var req supplier.GetLatestPricesReq
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()

	res, err := h.poGW.GetLatestPrices(ctx, req.IngredientIds)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}
