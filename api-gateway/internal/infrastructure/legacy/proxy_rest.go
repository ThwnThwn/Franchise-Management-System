package legacy

import (
	"net/http"
	"net/http/httputil"
	"net/url"

	"github.com/gin-gonic/gin"
)

func NewReverseProxy(target string) gin.HandlerFunc {
	return func(c *gin.Context) {
		remote, err := url.Parse(target)
		if err != nil {
			c.JSON(http.StatusInternalServerError, gin.H{"error": "URL config error"})
			return
		}

		proxy := httputil.NewSingleHostReverseProxy(remote)

		// 1. Lưu lại cái Director mặc định của thư viện (cái này có logic X-Forwarded-For)
		originalDirector := proxy.Director

		// 2. Viết hàm đè (Override)
		proxy.Director = func(req *http.Request) {
			// Gọi lại hàm cũ để nó tự làm việc:
			// - Tự thêm X-Forwarded-For
			// - Tự ghép URL path
			originalDirector(req)

			// 3. Sau đó mình chỉ sửa thêm cái Header Host thôi
			// (Để thằng .NET không từ chối request)
			req.Header = c.Request.Header
			req.Host = remote.Host

			// Fix lỗi User-Agent (Optional)
			// Đôi khi Go tự thêm User-Agent là "Go-http-client", mình nên giữ nguyên của Browser
			if _, ok := req.Header["User-Agent"]; !ok {
				req.Header.Set("User-Agent", "")
			}
		}

		// 2. Xử lý lỗi khi Service con bị chết (Quan trọng cho Prod)
		proxy.ErrorHandler = func(w http.ResponseWriter, req *http.Request, err error) {
			// Không dùng c.JSON được ở đây vì response writer đã bị hijack bởi proxy
			w.Header().Set("Content-Type", "application/json")
			w.WriteHeader(http.StatusBadGateway)
			w.Write([]byte(`{"error": "Service Unavailable", "message": "Cannot connect to sub-service"}`))
		}

		proxy.ServeHTTP(c.Writer, c.Request)
	}
}
