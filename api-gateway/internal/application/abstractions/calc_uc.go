package abstractions

import "context"

type ICalcUseCase interface {
	ExecuteAdd(ctx context.Context, a int32, b int32) (int32, error)
}
