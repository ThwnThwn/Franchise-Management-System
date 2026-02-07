package handlers

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/product"
	"api-gateway/internal/domain/models/category"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
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

func (h *CategoryHandler) GetById(c *gin.Context) {
	id := c.Param("id")
	cateId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	ctx := c.Request.Context()
	res, err := h.categoryGW.GetById(ctx, cateId)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

func (h *CategoryHandler) Create(c *gin.Context) {
	var req category.MutationCategoryReq
	if err := c.ShouldBindJSON(req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx := c.Request.Context()
	err := h.categoryGW.Create(ctx, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}

func (h *CategoryHandler) UpdateById(c *gin.Context) {
	id := c.Param("id")
	cateId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	var req category.MutationCategoryReq
	if err := c.ShouldBindJSON(req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx := c.Request.Context()
	err = h.categoryGW.Update(ctx, cateId, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}

func (h *CategoryHandler) DeleteById(c *gin.Context) {
	id := c.Param("id")
	cateId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	ctx := c.Request.Context()
	err = h.categoryGW.Delete(ctx, cateId)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}
