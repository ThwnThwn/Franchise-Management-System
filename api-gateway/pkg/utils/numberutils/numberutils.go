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

func IntToInt32Ptr(i *int) *int32 {
	if i == nil {
		return nil
	}
	v := int32(*i)
	return &v
}

func Int32ToIntPtr(i *int32) *int {
	if i == nil {
		return nil
	}
	v := int(*i)
	return &v
}

func EnumPtrToInt32Ptr[T ~int32](enumPtr *T) *int32 {
	if enumPtr == nil {
		return nil
	}
	val := int32(*enumPtr)
	return &val
}
