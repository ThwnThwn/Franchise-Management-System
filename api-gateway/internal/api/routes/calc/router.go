package calc

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"

	"github.com/gin-gonic/gin"
)

type CalcRouter struct{}

func (c *CalcRouter) NewCalcRouter(cfg *config.Config,
	r *gin.RouterGroup,
	calcHandler *handlers.CalcHandler,
) {
	calcApi := r.Group("/calc")

	calcApi.POST("/add", calcHandler.AddNumbers)
}
