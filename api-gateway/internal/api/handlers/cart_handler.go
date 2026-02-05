package handlers

import (
	"api-gateway/internal/application/gateways/sales"
	"api-gateway/internal/domain/models/cart"
	"api-gateway/pkg/utils/grpcutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
)

type CartHandler struct {
	cartGW sales.ICartService
}

func NewCartHandler(cartGW sales.ICartService) *CartHandler {
	return &CartHandler{cartGW: cartGW}
}

func (h *CartHandler) GetCartItems(c *gin.Context) {
	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		c.Error(err)
		return
	}

	res, err := h.cartGW.GetCartItems(ctx)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

func (h *CartHandler) AddToCart(c *gin.Context) {
	var req *cart.UpsertCartItemRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		c.Error(err)
		return
	}

	res, err := h.cartGW.AddToCart(ctx, req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

func (h *CartHandler) UpdateCartItem(c *gin.Context) {
	var req *cart.UpsertCartItemRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		c.Error(err)
		return
	}

	res, err := h.cartGW.UpdateCartItem(ctx, req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

func (h *CartHandler) DeleteListCartItem(c *gin.Context) {
	var req *cart.DeleteCartItemsRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		c.Error(err)
		return
	}

	isSuccess, err := h.cartGW.DeleteListCartItem(ctx, req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"success": isSuccess,
	})
}

func (h *CartHandler) ClearCart(c *gin.Context) {
	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		c.Error(err)
		return
	}

	isSuccess, err := h.cartGW.ClearCart(ctx)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"success": isSuccess,
	})
}
