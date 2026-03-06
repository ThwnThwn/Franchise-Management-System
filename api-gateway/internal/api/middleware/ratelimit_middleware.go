package middleware

import (
	"api-gateway/internal/application/constants/errorcode"
	"sync"
	"time"

	"github.com/gin-gonic/gin"
	"golang.org/x/time/rate"
)

type visitor struct {
	Limiter  *rate.Limiter
	LastSeen time.Time
	mu       sync.Mutex
}

var visitors sync.Map

func getLimiter(key string, r rate.Limit, b int) *rate.Limiter {
	// Load thử xem có chưa
	v, exists := visitors.Load(key)

	if !exists {
		limiter := rate.NewLimiter(r, b)
		newVisitor := &visitor{Limiter: limiter, LastSeen: time.Now()}

		actual, loaded := visitors.LoadOrStore(key, newVisitor)
		if loaded {
			v = actual // Đã bị thằng khác tạo trước
		} else {
			return limiter
		}
	}

	// Phải ép kiểu (Type Assertion) vì sync.Map lưu giá trị dưới dạng interface{} (any)
	vis := v.(*visitor)
	vis.mu.Lock()
	vis.LastSeen = time.Now()
	vis.mu.Unlock()

	return vis.Limiter
}

func RateLimitMiddleware(r rate.Limit, b int) gin.HandlerFunc {
	return func(c *gin.Context) {
		key := c.ClientIP()

		limiter := getLimiter(key, r, b)
		if !limiter.Allow() {
			c.Error(errorcode.NewTooManyRequest("Vui lòng thử lại sau"))
			c.Abort()
			return
		}
		c.Next()
	}
}

func StartCleanupJob(expireAfter time.Duration, interval time.Duration) {
	go func() {
		for {
			time.Sleep(interval)

			// Dùng Range để duyệt qua sync.Map một cách an toàn
			visitors.Range(func(key, value any) bool {
				vis := value.(*visitor)

				vis.mu.Lock()
				isExpired := time.Since(vis.LastSeen) > expireAfter
				vis.mu.Unlock()

				if isExpired {
					visitors.Delete(key)
				}
				return true // Trả về true để tiếp tục vòng lặp
			})
		}
	}()
}
