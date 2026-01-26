package handlers

import (
	"api-gateway/internal/application/abstractions"
	"net/http"

	"github.com/gin-gonic/gin"
)

type CategoryHandler struct {
	categoryUC abstractions.ICategoryUseCase
}

func NewCategoryHandler(categoryUC abstractions.ICategoryUseCase) *CategoryHandler {
	return &CategoryHandler{categoryUC: categoryUC}
}

func (h *CategoryHandler) GetCategories(c *gin.Context) {
	ctx := c.Request.Context()
	res, err := h.categoryUC.GetCategories(ctx)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}
