package franchise

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/domain/models/store"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type StoreHandler struct {
	storeGW franchise.IStoreService
}

func NewStoreHandler(storeGW franchise.IStoreService) *StoreHandler {
	return &StoreHandler{storeGW: storeGW}
}

// GetStores
// @Summary      Get All Stores
// @Description  Retrieve a list of stores with optional filtering (e.g., by city, district).
// @Tags         Stores
// @Accept       json
// @Produce      json
// @Param        query  query     store.GetStoresRequest  false  "Filter options"
// @Success      200    {object}  map[string]interface{}  "List of stores"
// @Failure      500    {object}  map[string]string       "Internal server error"
// @Router       /stores [get]
func (h *StoreHandler) GetStores(c *gin.Context) error {
	var req store.GetStoresRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		return err
	}

	ctx := c.Request.Context()
	res, err := h.storeGW.GetStores(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// FindNearbyStores
// @Summary      Find Nearby Stores
// @Description  Find stores based on current location (Lat/Long) and radius.
// @Tags         Stores
// @Accept       json
// @Produce      json
// @Param        query  query     store.FindNearbyStoresRequest  false  "Location params (Lat, Long, Radius)"
// @Success      200    {object}  map[string]interface{}         "List of nearby stores"
// @Failure      400    {object}  map[string]string              "Validation error"
// @Router       /stores/nearby [get]
func (h *StoreHandler) FindNearbyStores(c *gin.Context) error {
	var req *store.FindNearbyStoresRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		return err
	}

	ctx := c.Request.Context()
	res, err := h.storeGW.FindNearbyStores(ctx, req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetStoreDetail
// @Summary      Get Store Details
// @Description  Retrieve full details of a specific store by its ID.
// @Tags         Stores
// @Accept       json
// @Produce      json
// @Param        id   path      string  true  "Store UUID"
// @Success      200  {object}  map[string]interface{} "Store details"
// @Failure      400  {object}  map[string]string      "Invalid ID format"
// @Failure      404  {object}  map[string]string      "Store not found"
// @Router       /stores/{id} [get]
func (h *StoreHandler) GetStoreDetail(c *gin.Context) error {
	id := c.Param("id")
	storeId, err := uuid.Parse(id)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()
	res, err := h.storeGW.GetStoreDetail(ctx, storeId)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// CheckStoreAvailability
// @Summary      Check Store Availability
// @Description  Check if a store is currently open or active.
// @Tags         Stores
// @Accept       json
// @Produce      json
// @Param        id   path      string  true  "Store UUID"
// @Success      200  {object}  map[string]interface{} "Availability status"
// @Failure      400  {object}  map[string]string      "Invalid ID format"
// @Router       /stores/{id}/availability [get]
func (h *StoreHandler) CheckStoreAvailability(c *gin.Context) error {
	id := c.Param("id")
	storeId, err := uuid.Parse(id)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()
	isAvailable, msg, err := h.storeGW.CheckStoreAvailability(ctx, storeId)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{
		"isAvailable": isAvailable,
		"message":     msg,
	})
	return nil
}
