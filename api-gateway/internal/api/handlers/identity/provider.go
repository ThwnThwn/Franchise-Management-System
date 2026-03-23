package identity

import "github.com/google/wire"

var ProviderSet = wire.NewSet(
	NewAuthHandler,
	NewUserHandler,
	NewRoleHandler,
	NewPermHandler,
)
