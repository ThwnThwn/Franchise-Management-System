package handlers

import (
	"api-gateway/internal/application/abstractions"
	"api-gateway/internal/domain/models/cart"
	"api-gateway/pkg/utils/grpcutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
)

type CartHandler struct {
	cartUC abstractions.ICartUseCase
}

func NewCartHandler(cartUC abstractions.ICartUseCase) *CartHandler {
	return &CartHandler{cartUC: cartUC}
}

func (h *CartHandler) GetCartItems(c *gin.Context) {
	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		c.Error(err)
		return
	}

	res, err := h.cartUC.GetCartItems(ctx)
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

	res, err := h.cartUC.AddToCart(ctx, req)
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

	res, err := h.cartUC.UpdateCartItem(ctx, req)
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

	isSuccess, err := h.cartUC.DeleteListCartItem(ctx, req)
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

	isSuccess, err := h.cartUC.ClearCart(ctx)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"success": isSuccess,
	})
}
