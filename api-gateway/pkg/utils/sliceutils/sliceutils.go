package sliceutils

import (
	"slices"
)

// CollectUniqueIDs: Lấy ID từ mảng T, lọc trùng và bỏ trống
func CollectUniqueIDs[T any](items []T, selector func(T) string) []string {
	var ids []string
	for _, item := range items {
		if id := selector(item); id != "" {
			ids = append(ids, id)
		}
	}

	// Sắp xếp trước khi Compact
	slices.Sort(ids)
	return slices.Compact(ids) // Compact chỉ loại bỏ các phần tử trùng nhau KỀ NHAU
}
