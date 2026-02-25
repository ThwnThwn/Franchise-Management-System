package boolutils

func GetBool(p *bool) bool {
	if p != nil {
		return *p
	}
	return true
}
