package bootstrap

import (
	"api-gateway/config"
	"context"
	"fmt"
	"net/http"
	"os"
	"os/signal"
	"syscall"
	"time"

	"github.com/gin-gonic/gin"
)

type App struct {
	Config *config.Config
	Server *gin.Engine
}

func NewApp(cfg *config.Config, server *gin.Engine) *App {
	return &App{
		Config: cfg,
		Server: server,
	}
}

func (a *App) Run() error {
	port := a.Config.Port

	srv := &http.Server{
		Addr:    ":" + port,
		Handler: a.Server, // a.Server chính là *gin.Engine
	}

	fmt.Println("==========================================================")
	fmt.Printf("API Gateway is running on port: %s\n", port)
	fmt.Printf("Swagger UI: http://localhost:%s/swagger\n", port)
	fmt.Println("==========================================================")

	// Chạy server trong một Goroutine để nó không block luồng chính
	go func() {
		if err := srv.ListenAndServe(); err != nil && err != http.ErrServerClosed {
			fmt.Printf("Listen error: %s\n", err)
		}
	}()

	// Lắng nghe tín hiệu tắt máy từ hệ điều hành (Ctrl+C, Docker Stop)
	quit := make(chan os.Signal, 1)
	signal.Notify(quit, syscall.SIGINT, syscall.SIGTERM)

	// Code sẽ bị block ở dòng này, chờ cho đến khi có tín hiệu tắt máy
	<-quit
	fmt.Println("\nReceived shutdown signal...")

	// Tạo timeout 5 giây để các request đang chạy dở có thời gian hoàn thành
	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()

	fmt.Println("Shutting down Gin server...")
	if err := srv.Shutdown(ctx); err != nil {
		return fmt.Errorf("Server forced to shutdown: %w", err)
	}

	return nil
}
