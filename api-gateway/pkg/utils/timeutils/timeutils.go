package timeutils

import (
	"time"

	"google.golang.org/protobuf/types/known/timestamppb"
)

// Chuyển chuỗi FE gửi thành gRPC Timestamp (Chuẩn UTC)
func ParseStringToTimestamp(s *string) *timestamppb.Timestamp {
	if s == nil {
		return nil
	}

	// Mặc định location là Việt Nam (+07:00)
	loc, err := time.LoadLocation("Asia/Ho_Chi_Minh")
	if err != nil {
		loc = time.FixedZone("ICT", 7*3600) // Fallback cứng +7 nếu server thiếu data múi giờ
	}

	formats := []string{
		time.RFC3339, // Có sẵn Z hoặc múi giờ -> Tôn trọng múi giờ gốc
		time.RFC3339Nano,
		"2006-01-02T15:04:05", // Thiếu múi giờ -> Gán giờ VN
		time.DateTime,         // Thiếu múi giờ -> Gán giờ VN
		time.DateOnly,         // Thiếu múi giờ -> Gán giờ VN
	}

	for _, format := range formats {
		if t, err := time.ParseInLocation(format, *s, loc); err == nil {
			// Bất chấp là giờ VN hay giờ gì, ép tất cả về UTC (Z) rồi gói vào Protobuf
			return timestamppb.New(t.UTC())
		}
	}

	return nil
}

// ParseStringPtrToTimestamp nhận con trỏ chuỗi FE gửi thành gRPC Timestamp (Chuẩn UTC)
func ParseStringPtrToTimestamp(s *string) *timestamppb.Timestamp {
	// 1. Chặn luôn 2 bãi mìn: Không gửi gì (nil) HOẶC gửi chuỗi rỗng ("")
	if s == nil || *s == "" {
		return nil
	}

	// Mặc định location là Việt Nam (+07:00)
	loc, err := time.LoadLocation("Asia/Ho_Chi_Minh")
	if err != nil {
		loc = time.FixedZone("ICT", 7*3600) // Fallback cứng +7 nếu server thiếu data múi giờ
	}

	formats := []string{
		time.RFC3339, // Có sẵn Z hoặc múi giờ -> Tôn trọng múi giờ gốc
		time.RFC3339Nano,
		"2006-01-02T15:04:05", // Thiếu múi giờ -> Gán giờ VN
		time.DateTime,         // Thiếu múi giờ -> Gán giờ VN
		time.DateOnly,         // Thiếu múi giờ -> Gán giờ VN
	}

	for _, format := range formats {
		// 2. Dereference (giải nén) con trỏ bằng *s an toàn vì đã check nil ở trên
		if t, err := time.ParseInLocation(format, *s, loc); err == nil {
			// Bất chấp là giờ VN hay giờ gì, ép tất cả về UTC (Z) rồi gói vào Protobuf
			return timestamppb.New(t.UTC())
		}
	}

	return nil
}

// Chuyển gRPC Timestamp (UTC) thành chuỗi ISO 8601 có đuôi Z
func ParseTimestampToString(ts *timestamppb.Timestamp) string {
	if ts == nil || !ts.IsValid() {
		return ""
	}

	// Mặc định ts.AsTime() đã hiểu là UTC rồi.
	// Hàm .UTC() gọi thêm cho chắc cốp, và Format ra chuỗi ISO có đuôi 'Z'
	return ts.AsTime().UTC().Format(time.RFC3339)
}

// ParseTimestampToStringPtr chuyển gRPC Timestamp thành con trỏ chuỗi ISO 8601.
// Rất hữu ích cho các field JSON có tag omitempty (trả về nil thay vì chuỗi rỗng "").
func ParseTimestampToStringPtr(ts *timestamppb.Timestamp) *string {
	// Kiểm tra nil hoặc không hợp lệ thì trả thẳng về con trỏ rỗng (nil)
	if ts == nil || !ts.IsValid() {
		return nil
	}

	// Tận dụng lại hàm cũ để lấy chuỗi ISO chuẩn Z
	isoString := ParseTimestampToString(ts)

	// Trả về địa chỉ của chuỗi đó
	return &isoString
}

func ProtoToTimePtr(ts *timestamppb.Timestamp) *time.Time {
	if ts == nil {
		return nil
	}
	t := ts.AsTime()
	return &t
}

func TimeToProto(t *time.Time) *timestamppb.Timestamp {
	if t == nil {
		return nil
	}
	return timestamppb.New(*t)
}

func StringToTimePtr(s string) *time.Time {
	if s == "" {
		return nil
	}

	if t, err := time.Parse(time.RFC3339, s); err == nil {
		return &t
	}

	if t, err := time.Parse(time.DateOnly, s); err == nil {
		return &t
	}

	return nil
}

// Dành cho trường hợp biến trong struct Go là con trỏ *time.Time
func TimePtrToString(t *time.Time) string {
	if t == nil {
		return ""
	}
	return t.Format(time.RFC3339)
}

func StringToTime(s string) time.Time {
	if s == "" {
		return time.Time{}
	}

	if t, err := time.Parse(time.RFC3339, s); err == nil {
		return t
	}
	if t, err := time.Parse(time.DateOnly, s); err == nil {
		return t
	}
	return time.Time{}
}
