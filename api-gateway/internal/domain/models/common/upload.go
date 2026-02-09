package common

type UploadImageRequest struct {
	ImageData   []byte `json:"imageData"`
	FileName    string `json:"fileName"`
	ContentType string `json:"contentType"`
}
