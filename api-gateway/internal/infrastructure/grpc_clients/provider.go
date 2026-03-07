package grpc_clients

import (
	"api-gateway/internal/infrastructure/grpc_clients/engagement"
	"api-gateway/internal/infrastructure/grpc_clients/franchise"
	"api-gateway/internal/infrastructure/grpc_clients/identity"
	"api-gateway/internal/infrastructure/grpc_clients/inventory"
	"api-gateway/internal/infrastructure/grpc_clients/product"
	"api-gateway/internal/infrastructure/grpc_clients/sales"
	"api-gateway/internal/infrastructure/grpc_clients/supplier"

	"github.com/google/wire"
)

var ClientSet = wire.NewSet(
	// Engagement Service
	engagement.NewEngagementClientFactory,
	engagement.ProvideLoyaltyAccountClient,
	engagement.ProvideMembershipTierClient,
	engagement.ProvidePointLogClient,
	engagement.ProvideVoucherClient,

	// Franchise Service
	franchise.NewFranchiseClientFactory,
	franchise.ProvideShiftAvailabilityClient,
	franchise.ProvideShiftClient,
	franchise.ProvideShiftRuleClient,
	franchise.ProvideShiftTemplateClient,
	franchise.ProvideStoreClient,

	// Identity Service
	identity.NewIdentityClientFactory,
	identity.ProvideAuthClient,
	identity.ProvideUserClient,
	identity.ProvideRolePermClient,
	identity.ProvideRoleClient,

	// Product Service
	product.NewProductClientFactory,
	product.ProvideCategoryClient,
	product.ProvideProductClient,

	// Sales Service
	sales.NewSalesClientFactory,
	sales.ProvideOrderClient,
	sales.ProvidePaymentClient,
	sales.ProvideCartClient,
	sales.ProvideRefundTicketClient,
	sales.ProvideWalletClient,
	sales.ProvideWalletTransactionClient,
	sales.ProvideWithdrawalClient,

	// Inventory Service
	inventory.NewInventoryClientFactory,
	inventory.ProvideStoreIngredientClient,
	inventory.ProvideIngredientClient,
	inventory.ProvideIngredientCategoryClient,
	inventory.ProvideRecipeClient,

	// Supplier
	supplier.NewSupplierClientFactory,
	supplier.NewSupplierClient,
	supplier.NewPurchaseOrderClient,
	supplier.NewSupplierPaymentClient,
)
