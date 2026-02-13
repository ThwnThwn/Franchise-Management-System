package product

import (
	"api-gateway/internal/application/constants/errorcode"
	prodGW "api-gateway/internal/application/gateways/product"
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/product"
	"api-gateway/pkg/utils/stringutils"
	"api-gateway/pkg/utils/validation"
	"io"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type ProductHandler struct {
	productGW prodGW.IProductService
}

func NewProductHandler(productGW prodGW.IProductService) *ProductHandler {
	return &ProductHandler{productGW: productGW}
}

// GetProducts
// @Summary      Get All Products
// @Description  Retrieve a list of products with pagination and keyword search support.
// @Tags         Products
// @Accept       json
// @Produce      json
// @Param        query  query     product.GetProductsByKeywordRequest  false  "Filter options"
// @Success      200    {object}  map[string]interface{}               "List of products"
// @Failure      500    {object}  map[string]string                    "Internal server error"
// @Router       /products [get]
func (h *ProductHandler) GetProducts(c *gin.Context) {
	var req product.GetProductsByKeywordRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		c.Error(err)
		return
	}

	ctx := c.Request.Context()
	res, err := h.productGW.GetAllWithPagingBy(ctx, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

// GetProductById
// @Summary      Get Product Details
// @Description  Retrieve full details of a specific product by its ID.
// @Tags         Products
// @Accept       json
// @Produce      json
// @Param        id   path      string  true  "Product UUID"
// @Success      200  {object}  map[string]interface{} "Product details"
// @Failure      400  {object}  map[string]string      "Invalid ID format"
// @Failure      404  {object}  map[string]string      "Product not found"
// @Router       /products/{id} [get]
func (h *ProductHandler) GetProductById(c *gin.Context) {
	id := c.Param("id")
	productId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	ctx := c.Request.Context()
	res, err := h.productGW.GetProductById(ctx, productId)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

// CreateProduct
// @Summary      Create New Product
// @Description  Add a new product to the catalog.
// @Tags         Products
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body     product.CreateProductRequest  true  "Product details"
// @Success      200     {object} map[string]interface{}        "Success"
// @Failure      400     {object} map[string]string             "Validation error"
// @Router       /products [post]
func (h *ProductHandler) CreateProduct(c *gin.Context) {
	var req product.CreateProductRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx := c.Request.Context()
	err := h.productGW.CreateProduct(ctx, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}

// UpdateProduct
// @Summary      Update Product
// @Description  Update details of an existing product.
// @Tags         Products
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                        true  "Product UUID"
// @Param        request  body     product.UpdateProductRequest  true  "Updated details"
// @Success      200      {object} map[string]interface{}        "Success"
// @Failure      400      {object} map[string]string             "Validation error or Invalid ID"
// @Router       /products/{id} [patch]
func (h *ProductHandler) UpdateProduct(c *gin.Context) {
	var req product.UpdateProductRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	id := c.Param("id")
	productId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	ctx := c.Request.Context()
	err = h.productGW.UpdateProduct(ctx, productId, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}

// DeleteProduct
// @Summary      Delete Product
// @Description  Permanently remove a product from the catalog.
// @Tags         Products
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id   path      string                  true  "Product UUID"
// @Success      200  {object}  map[string]interface{}  "Success"
// @Failure      400  {object}  map[string]string       "Invalid ID format"
// @Router       /products/{id} [delete]
func (h *ProductHandler) DeleteProduct(c *gin.Context) {
	id := c.Param("id")
	productId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	ctx := c.Request.Context()
	err = h.productGW.DeleteProduct(ctx, productId)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}

// UploadImage
// @Summary      Upload Product Image
// @Description  Upload an image for a specific product. Supports JPG, PNG, and WebP formats. Max file size is 10MB.
// @Tags         Products
// @Accept       multipart/form-data
// @Produce      json
// @Security     BearerAuth
// @Param        id         path      string  true   "Product UUID"
// @Param        image      formData  file    true   "Image file to upload"
// @Param        file_name  formData  string  false  "Custom file name (Optional)"
// @Success      200        {object}  map[string]string "Returns the uploaded image URL"
// @Failure      400        {object}  map[string]string "Invalid ID or unsupported image format"
// @Failure      413        {object}  map[string]string "Request entity too large"
// @Failure      500        {object}  map[string]string "Internal server error"
// @Router       /products/{id}/images [post]
func (h *ProductHandler) UploadImage(c *gin.Context) {
	id := c.Param("id")
	productId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	// 1. Lấy file từ form-data (key là "image")
	file, header, err := c.Request.FormFile("image")
	if err != nil {
		c.Error(errorcode.ErrImageRequired)
		return
	}
	defer file.Close()

	// 2. Đọc file vào byte slice
	fileBytes, err := io.ReadAll(file)
	if err != nil {
		c.Error(err)
		return
	}

	// 3. Validation: Kiểm tra xem có thực sự là ảnh không (Magic Bytes)
	contentType := http.DetectContentType(fileBytes)
	allowedTypes := map[string]bool{
		"image/jpeg": true,
		"image/png":  true,
		"image/webp": true,
	}
	if !allowedTypes[contentType] {
		c.Error(errorcode.ErrInvalidImageFormat)
		return
	}

	// 4. Sanitize tên file
	// Sử dụng productId làm salt để tên file mang tính định danh và không trùng
	salt := productId.String()
	rawFileName := header.Filename
	if customName := c.PostForm("file_name"); customName != "" {
		rawFileName = customName
	}
	fileName := stringutils.SanitizeFileName(rawFileName, &salt)

	// 5. Build request cho tầng Client
	req := &common.UploadImageRequest{
		ImageData:   fileBytes,
		FileName:    fileName,
		ContentType: contentType,
	}

	// 6. Gọi xuống Client (Gateway)
	ctx := c.Request.Context()
	url, err := h.productGW.UploadImage(ctx, productId, req)
	if err != nil {
		c.Error(err)
		return
	}

	// 7. Trả kết quả
	c.JSON(http.StatusOK, gin.H{
		"url": url,
	})
}
