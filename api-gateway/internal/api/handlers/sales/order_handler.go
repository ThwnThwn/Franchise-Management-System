package sales

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/sales"
	"api-gateway/internal/domain/constants/roleslug"
	"api-gateway/internal/domain/models/order"
	"api-gateway/pkg/utils/contextutils"
	"api-gateway/pkg/utils/grpcutils"
	"api-gateway/pkg/utils/stringutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type OrderHandler struct {
	orderGW sales.IOrderService
}

func NewOrderHandler(orderGW sales.IOrderService) *OrderHandler {
	return &OrderHandler{orderGW: orderGW}
}

// GetOrders
// @Summary      Get Orders History
// @Description  Retrieve a list of orders placed with optional filtering.
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        query  query     order.GetOrdersRequest  false  "Filter params (page, status, etc.)"
// @Success      200    {object}  map[string]interface{}    "List of orders"
// @Failure      401    {object}  map[string]string         "Unauthorized"
// @Router       /orders [get]
func (h *OrderHandler) GetOrders(c *gin.Context) {
	var req order.GetOrdersRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		c.Error(err)
		return
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		c.Error(err)
		return
	}

	roleSlug, err := contextutils.GetRoleSlug(c)
	if err != nil {
		c.Error(err)
		return
	}

	storeId, err := contextutils.GetStoreID(c)
	if err != nil {
		c.Error(err)
		return
	}

	if roleSlug == string(roleslug.Manager) || roleSlug == string(roleslug.Staff) {
		req.StoreId = stringutils.ToPtr(storeId.String())
	}

	res, err := h.orderGW.GetOrders(ctx, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

// GetOrderById
// @Summary      Get Order Details
// @Description  Retrieve full details of a specific order by its ID.
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id   path      string  true  "Order UUID"
// @Success      200  {object}  map[string]interface{} "Order details"
// @Failure      400  {object}  map[string]string      "Invalid ID format"
// @Failure      404  {object}  map[string]string      "Order not found"
// @Router       /orders/{id} [get]
func (h *OrderHandler) GetOrderById(c *gin.Context) {
	id := c.Param("id")
	orderId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		c.Error(err)
		return
	}

	res, err := h.orderGW.GetOrderById(ctx, orderId)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

// GetMyOrders
// @Summary      Get My Order History
// @Description  Retrieve a list of orders placed by the current logged-in user with optional filtering.
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        query  query     order.GetMyOrdersRequest  false  "Filter params (page, status, etc.)"
// @Success      200    {object}  map[string]interface{}    "List of orders"
// @Failure      401    {object}  map[string]string         "Unauthorized"
// @Router       /me/orders [get]
func (h *OrderHandler) GetMyOrders(c *gin.Context) {
	var req order.GetMyOrdersRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		c.Error(err)
		return
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		c.Error(err)
		return
	}

	res, err := h.orderGW.GetMyOrders(ctx, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

// CreateOrder
// @Summary      Place New Order
// @Description  Create a new order based on the provided items and shipping information.
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body     order.CreateOrderRequest  true  "Order creation details"
// @Success      200     {object} map[string]interface{}    "Created Order Data"
// @Failure      400     {object} map[string]string         "Validation error"
// @Router       /orders [post]
func (h *OrderHandler) CreateOrder(c *gin.Context) {
	var req order.CreateOrderRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		c.Error(err)
		return
	}

	res, err := h.orderGW.CreateOrder(ctx, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

// UpdateOrder
// @Summary      Update Order Items
// @Description  Update the list of items in an existing order (usually allowed only when order is in 'Pending' state).
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                    true  "Order UUID"
// @Param        request  body     []order.UpdateOrderItem   true  "List of items to update"
// @Success      200      {object} map[string]interface{}    "Updated Order Data"
// @Failure      400      {object} map[string]string         "Validation error"
// @Router       /orders/{id} [put]
func (h *OrderHandler) UpdateOrder(c *gin.Context) {
	id := c.Param("id")
	orderId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	var req []*order.UpdateOrderItem
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		c.Error(err)
		return
	}

	res, err := h.orderGW.UpdateOrder(ctx, orderId, req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

// UpdateOrderStatus
// @Summary      Update Order Status
// @Description  Change the status of an order (e.g., to 'Cancelled', 'Completed').
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                   true  "Order UUID"
// @Param        request  body     order.UpdateOrderStatus  true  "New Status"
// @Success      200      {object} map[string]interface{}   "Success"
// @Failure      400      {object} map[string]string        "Validation error"
// @Router       /orders/{id}/status [patch]
func (h *OrderHandler) UpdateOrderStatus(c *gin.Context) {
	id := c.Param("id")
	orderId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	var req order.UpdateOrderStatus
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		c.Error(err)
		return
	}

	err = h.orderGW.UpdateStatus(ctx, orderId, req.Status)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}

// CreatePaymentUrl
// @Summary      Generate Payment URL
// @Description  Generate a payment link (e.g., VNPay, Momo) for the specific order.
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                    true  "Order UUID"
// @Param        request  body     order.CreatePaymentRequest true "Payment details"
// @Success      200      {object} map[string]interface{}    "Payment URL"
// @Failure      400      {object} map[string]string         "Validation error"
// @Router       /orders/{id}/payment [post]
func (h *OrderHandler) CreatePaymentUrl(c *gin.Context) {
	var req order.CreatePaymentRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	id := c.Param("id")
	orderId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		c.Error(err)
		return
	}

	res, err := h.orderGW.CreatePaymentUrl(ctx, orderId, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"paymentUrl": res,
	})
}
