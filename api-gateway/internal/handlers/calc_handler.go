package handlers

import (
	"context"
	"net/http"
	"time"

	"api-gateway/internal/clients"
	pb "api-gateway/pkg/pb/calc"

	"github.com/gin-gonic/gin"
)

type CalcHandler struct {
	client *clients.CalcServiceClient
}

// Constructor inject client vào handler
func NewCalcHandler(client *clients.CalcServiceClient) *CalcHandler {
	return &CalcHandler{client: client}
}

// Request Body Struct
type AddRequest struct {
	A int32 `json:"a" binding:"required"`
	B int32 `json:"b" binding:"required"`
}

func (h *CalcHandler) AddNumbers(c *gin.Context) {
	var body AddRequest
	if err := c.ShouldBindJSON(&body); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	ctx, cancel := context.WithTimeout(context.Background(), 2*time.Second)
	defer cancel()

	res, err := h.client.Client.Add(ctx, &pb.AddRequest{
		NumberA: body.A,
		NumberB: body.B,
	})

	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Error from Calc Service", "details": err.Error()})
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"result": res.Result,
	})
}
