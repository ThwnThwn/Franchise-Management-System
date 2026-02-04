//go:build wireinject
// +build wireinject

package di

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/routes"
	"api-gateway/internal/application/usecases"
	"api-gateway/internal/bootstrap"
	"api-gateway/internal/infrastructure/cache"
	"api-gateway/internal/infrastructure/grpc_clients"
	"api-gateway/pkg/logger"

	"github.com/google/wire"
)

// InitializeApp nằm ở package 'di', trả về 'bootstrap.App'
func InitializeApp(cfg *config.Config, l logger.Interface) (*bootstrap.App, error) {
	wire.Build(
		// 1. App
		bootstrap.NewApp,

		// 2. Trích xuất Config
		wire.FieldsOf(new(*config.Config), "Redis"),
		wire.FieldsOf(new(*config.Config), "IdentityServiceUrl"),
		wire.FieldsOf(new(*config.Config), "ProductServiceUrl"),
		wire.FieldsOf(new(*config.Config), "SalesServiceUrl"),
		wire.FieldsOf(new(*config.Config), "InventoryServiceUrl"),
		wire.FieldsOf(new(*config.Config), "FranchiseServiceUrl"),

		// 3. Infrastructure
		cache.NewRedis,
		cache.NewBlacklistCache,

		// 4. Sub sets
		grpc_clients.ClientSet,
		usecases.UseCaseSets,
		handlers.HandlerSet,

		// 4.5. Wire auto group all the handlers
		wire.Struct(new(handlers.AppHandlers), "*"),

		// 5. Router
		routes.NewRouter,
	)
	return &bootstrap.App{}, nil
}
