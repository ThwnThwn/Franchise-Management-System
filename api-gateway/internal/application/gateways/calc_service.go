package gateways

import "context"

type ICalcService interface {
	Add(ctx context.Context, a int32, b int32) (int32, error)
}
