package identity

import "github.com/google/wire"

type IdentityHandlers struct {
	*AuthHandler
	*UserHandler
	*RoleHandler
	*PermHandler
}

func NewIdentityHandlers(
	auth *AuthHandler,
	user *UserHandler,
	role *RoleHandler,
	perm *PermHandler,
) IdentityHandlers {
	return IdentityHandlers{
		AuthHandler: auth,
		UserHandler: user,
		RoleHandler: role,
		PermHandler: perm,
	}
}

var ProviderSet = wire.NewSet(
	NewAuthHandler,
	NewUserHandler,
	NewRoleHandler,
	NewPermHandler,
	NewIdentityHandlers,
)
