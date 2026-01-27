package numberutils

func GetInt32(p *int32) int32 {
	if p == nil {
		return 0
	}
	return *p
}

func GetFloat64(p *float64) float64 {
	if p == nil {
		return 0
	}
	return *p
}
