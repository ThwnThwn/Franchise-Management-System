package handlers

import (
	"api-gateway/internal/application/abstractions"
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/domain/models/order"
	"api-gateway/pkg/utils/grpcutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type OrderHandler struct {
	orderUC abstractions.IOrderUseCase
}

func NewOrderHandler(orderUC abstractions.IOrderUseCase) *OrderHandler {
	return &OrderHandler{orderUC: orderUC}
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

	res, err := h.orderUC.CreateOrder(ctx, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

func (h *OrderHandler) GetOrderById(c *gin.Context) {
	id := c.Param("id")
	orderId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	ctx := c.Request.Context()
	res, err := h.orderUC.GetOrderById(ctx, orderId)
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

	res, err := h.orderUC.GetMyOrders(ctx, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

func (h *OrderHandler) CreatePaymentUrl(c *gin.Context) {
	id := c.Param("id")
	orderId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	ctx := c.Request.Context()
	res, err := h.orderUC.CreatePaymentUrl(ctx, orderId)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"paymentUrl": res,
	})
}
