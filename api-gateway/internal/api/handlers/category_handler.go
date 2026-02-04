package handlers

import (
	"api-gateway/internal/application/gateways/product"
	"net/http"

	"github.com/gin-gonic/gin"
)

type CategoryHandler struct {
	categoryGW product.ICategoryService
}

func NewCategoryHandler(categoryGW product.ICategoryService) *CategoryHandler {
	return &CategoryHandler{categoryGW: categoryGW}
}

func (h *CategoryHandler) GetCategories(c *gin.Context) {
	ctx := c.Request.Context()
	res, err := h.categoryGW.GetCategories(ctx)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}
