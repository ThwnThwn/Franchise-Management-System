package engagement

import (
	"api-gateway/internal/application/gateways/engagement"
	_ "api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/point"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
)

type PointHandler struct {
	pointGW engagement.IPointLogService
}

func NewPointHandler(pointGW engagement.IPointLogService) *PointHandler {
	return &PointHandler{
		pointGW: pointGW,
	}
}

// GetMyPointLogs
// @Summary      Get My Point History
// @Description  Retrieve pagination list of point transaction logs for the current authenticated user.
// @Tags         Points
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        query  query     point.GetMyPointLogRequest  false  "Pagination and filter params"
// @Success      200    {object}  common.PaginationResult[point.LogResponse] "List of point logs"
// @Failure      401    {object}  map[string]string                          "Unauthorized"
// @Router       /me/points/logs [get]
func (h *PointHandler) GetMyPointLogs(c *gin.Context) error {
	var req point.GetMyPointLogRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	res, err := h.pointGW.GetMyPointLog(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}
