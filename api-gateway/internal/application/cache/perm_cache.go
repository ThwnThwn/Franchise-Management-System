package cache

import "context"

type IPermCache interface {
	IsRoleLoaded(ctx context.Context, roleSlug string) (bool, error)
	HasPermission(ctx context.Context, roleSlug string, permSlug string) (bool, error)
}
