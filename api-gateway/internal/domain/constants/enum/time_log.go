package enum

type TimeLogStatus int

const (
	TimeLogStatus_PROCESSING TimeLogStatus = iota // 0
	TimeLogStatus_VALID                           // 1
	TimeLogStatus_INVALID                         // 2
)
