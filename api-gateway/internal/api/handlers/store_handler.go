package handlers

import (
	"api-gateway/internal/application/abstractions"
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/domain/models/store"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type StoreHandler struct {
	storeUC abstractions.IStoreUseCase
}

func NewStoreHandler(storeUC abstractions.IStoreUseCase) *StoreHandler {
	return &StoreHandler{storeUC: storeUC}
}

func (h *StoreHandler) GetStores(c *gin.Context) {
	var req store.GetStoresRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		c.Error(err)
		return
	}

	ctx := c.Request.Context()
	res, err := h.storeUC.GetStores(ctx, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

func (h *StoreHandler) GetStoreDetail(c *gin.Context) {
	id := c.Param("id")
	storeId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	ctx := c.Request.Context()
	res, err := h.storeUC.GetStoreDetail(ctx, storeId)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

func (h *StoreHandler) FindNearbyStores(c *gin.Context) {
	var req *store.FindNearbyStoresRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		c.Error(err)
		return
	}

	ctx := c.Request.Context()
	res, err := h.storeUC.FindNearbyStores(ctx, req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

func (h *StoreHandler) CheckStoreAvailability(c *gin.Context) {
	id := c.Param("id")
	storeId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	ctx := c.Request.Context()
	isAvailable, msg, err := h.storeUC.CheckStoreAvailability(ctx, storeId)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"isAvailable": isAvailable,
		"message":     msg,
	})
}
