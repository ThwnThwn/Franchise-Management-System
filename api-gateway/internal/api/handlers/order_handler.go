package handlers

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/sales"
	"api-gateway/internal/domain/models/order"
	"api-gateway/pkg/utils/grpcutils"
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
