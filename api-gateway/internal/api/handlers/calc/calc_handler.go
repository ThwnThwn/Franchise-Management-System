package calc

import (
	"context"
	"net/http"
	"time"

	"api-gateway/internal/api/handlers/calc/request"
	"api-gateway/internal/application/abstractions"

	"github.com/gin-gonic/gin"
)

type CalcHandler struct {
	calcUC abstractions.ICalcUseCase
}

// Constructor inject client vào handler
func NewCalcHandler(calcUC abstractions.ICalcUseCase) *CalcHandler {
	return &CalcHandler{calcUC: calcUC}
}

func (h *CalcHandler) AddNumbers(c *gin.Context) {
	var body request.AddRequest
	if err := c.ShouldBindJSON(&body); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	ctx, cancel := context.WithTimeout(context.Background(), 2*time.Second)
	defer cancel()

	res, err := h.calcUC.ExecuteAdd(ctx, body.A, body.B)

	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{
			"error":   "Error from Calc Service",
			"details": err.Error(),
		})
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"result": res,
	})
}
