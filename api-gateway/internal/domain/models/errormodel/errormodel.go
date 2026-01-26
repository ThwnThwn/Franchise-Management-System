package errormodel

type BackendError struct {
	Title  string `json:"title"`
	Status int    `json:"status"`
	Detail string `json:"detail"`
}

func (e *BackendError) Error() string {
	return e.Detail
}
