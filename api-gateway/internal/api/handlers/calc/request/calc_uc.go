package request

// Request Body Struct
type AddRequest struct {
	A int32 `json:"a" binding:"required"`
	B int32 `json:"b" binding:"required"`
}
