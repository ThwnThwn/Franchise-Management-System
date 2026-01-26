package timeutils

import (
	"time"

	"google.golang.org/protobuf/types/known/timestamppb"
)

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
