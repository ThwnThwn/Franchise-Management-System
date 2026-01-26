package calc

import (
	"context"
	"net/http"
	"time"

	"api-gateway/internal/api/handlers/calc/request"
	"api-gateway/internal/application/abstractions"
	"api-gateway/pkg/utils/validation"

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
	var req request.AddRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx, cancel := context.WithTimeout(context.Background(), 2*time.Second)
	defer cancel()

	res, err := h.calcUC.ExecuteAdd(ctx, req.A, req.B)

	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"result": res,
	})
}
