package points

import (
	"api-gateway/internal/api"

	"github.com/gin-gonic/gin"
)

func (l *PointLogRouter) Init(r *gin.RouterGroup) {
	meApi := r.Group("/me")

	meApi.Use(l.authMW.ValidateToken([]byte(l.cfg.JwtSecret)))
	{
		// GET /me/points/logs
		meApi.GET("/points/logs", api.WrapHandler(l.pointHl.GetMyPointLogs))
	}
}
