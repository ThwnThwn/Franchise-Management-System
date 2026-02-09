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

// GetMyCart
// @Summary      Get My Cart
// @Description  Retrieve current user's shopping cart.
// @Tags         Cart
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Router       /me/cart [get]
func (h *CartHandler) GetMyCart(c *gin.Context) {
	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		c.Error(err)
		return
	}

	res, err := h.cartGW.GetMyCart(ctx)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

// AddToMyCart
// @Summary      Add Item to My Cart
// @Tags         Cart
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body cart.UpsertCartItemRequest true "Item details"
// @Router       /me/cart/items [post]
func (h *CartHandler) AddToMyCart(c *gin.Context) {
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

	res, err := h.cartGW.AddToMyCart(ctx, req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

// UpdateMyCartItems
// @Summary      Update Item in My Cart
// @Tags         Cart
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body cart.UpsertCartItemRequest true "Update details"
// @Router       /me/cart/items [put]
func (h *CartHandler) UpdateMyCartItems(c *gin.Context) {
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

	res, err := h.cartGW.UpdateMyCartItems(ctx, req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

// DeleteMyCartItems
// @Summary      Remove Items from My Cart
// @Tags         Cart
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body cart.DeleteCartItemsRequest true "List of Item IDs"
// @Router       /me/cart/items [delete]
func (h *CartHandler) DeleteMyCartItems(c *gin.Context) {
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

	isSuccess, err := h.cartGW.DeleteMyCartItems(ctx, req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"success": isSuccess,
	})
}

// ClearMyCart
// @Summary      Clear My Cart
// @Tags         Cart
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Router       /me/cart [delete]
func (h *CartHandler) ClearMyCart(c *gin.Context) {
	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		c.Error(err)
		return
	}

	isSuccess, err := h.cartGW.ClearMyCart(ctx)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"success": isSuccess,
	})
}
