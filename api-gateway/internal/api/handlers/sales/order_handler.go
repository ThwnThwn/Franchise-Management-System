package sales

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/application/gateways/identity"
	"api-gateway/internal/application/gateways/sales"
	"api-gateway/internal/domain/models/order"
	"api-gateway/pkg/mapper"
	"api-gateway/pkg/utils/contextutils"
	"api-gateway/pkg/utils/sliceutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type OrderHandler struct {
	orderGW sales.IOrderService
	storeGW franchise.IStoreService
	userGW  identity.IUserService
}

func NewOrderHandler(
	orderGW sales.IOrderService,
	storeGW franchise.IStoreService,
	userGW identity.IUserService,
) *OrderHandler {
	return &OrderHandler{
		orderGW: orderGW,
		storeGW: storeGW,
		userGW:  userGW,
	}
}

// GetOrders
// @Summary      Get Orders History
// @Description  Retrieve a list of orders placed with optional filtering.
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        query  query     order.GetOrdersRequest  false  "Filter params (page, status, etc.)"
// @Success      200    {object}  map[string]interface{}    "List of orders"
// @Failure      401    {object}  map[string]string         "Unauthorized"
// @Router       /orders [get]
func (h *OrderHandler) GetOrders(c *gin.Context) error {
	var req order.GetOrdersRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	contextutils.ApplyStoreBoundaries(c, &req.StoreId)

	res, err := h.orderGW.GetOrders(ctx, &req)
	if err != nil {
		return err
	}
	if len(res.Items) == 0 {
		c.JSON(http.StatusOK, res)
		return nil
	}

	storeIds := sliceutils.CollectUniqueIDs(res.Items, func(o *order.OrderSummary) string {
		if o.Store != nil {
			return o.Store.Id
		}
		return ""
	})

	stores, err := h.storeGW.GetByIds(ctx, storeIds)
	if err == nil {
		// Chỉ map nếu lấy được store, không thì trả về data gốc
		res.Items = mapper.MapStoreToOrderHeaders(res.Items, stores)
	}

	deliveryManIds := sliceutils.CollectUniqueIDs(res.Items, func(o *order.OrderSummary) string {
		if o.DeliveryMan != nil {
			return o.DeliveryMan.Id
		}
		return ""
	})

	deliveryMans, err := h.userGW.GetByIds(ctx, deliveryManIds)
	if err == nil {
		res.Items = mapper.MapDeliveryManToOrderHeaders(res.Items, deliveryMans)
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetOrderById
// @Summary      Get Order Details
// @Description  Retrieve full details of a specific order by its ID.
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id   path      string  true  "Order UUID"
// @Success      200  {object}  map[string]interface{} "Order details"
// @Failure      400  {object}  map[string]string      "Invalid ID format"
// @Failure      404  {object}  map[string]string      "Order not found"
// @Router       /orders/{id} [get]
func (h *OrderHandler) GetOrderById(c *gin.Context) error {
	id := c.Param("id")
	orderId, err := uuid.Parse(id)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()
	res, err := h.orderGW.GetOrderById(ctx, orderId)
	if err != nil {
		return err
	}

	if storeId, err := uuid.Parse(res.Store.Id); err == nil {
		store, err := h.storeGW.GetStoreDetail(ctx, storeId)
		if err == nil {
			res.Store = store
		}
	}

	if deliveryManId, err := uuid.Parse(res.DeliveryMan.Id); err == nil {
		deliveryMan, err := h.userGW.GetById(ctx, deliveryManId)
		if err == nil {
			res.DeliveryMan = &order.DeliveryMan{
				Id:        deliveryMan.Id,
				Username:  deliveryMan.Username,
				Email:     deliveryMan.Email,
				Phone:     deliveryMan.Phone,
				FirstName: deliveryMan.FirstName,
				LastName:  &deliveryMan.LastName,
				AvatarUrl: deliveryMan.AvatarUrl,
			}
		}
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetMyOrders
// @Summary      Get My Order History
// @Description  Retrieve a list of orders placed by the current logged-in user with optional filtering.
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        query  query     order.GetMyOrdersRequest  false  "Filter params (page, status, etc.)"
// @Success      200    {object}  map[string]interface{}    "List of orders"
// @Failure      401    {object}  map[string]string         "Unauthorized"
// @Router       /me/orders [get]
func (h *OrderHandler) GetMyOrders(c *gin.Context) error {
	var req order.GetMyOrdersRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		return err
	}

	ctx := c.Request.Context()
	res, err := h.orderGW.GetMyOrders(ctx, &req)
	if err != nil {
		return err
	}
	if len(res) == 0 {
		c.JSON(http.StatusOK, res)
		return nil
	}

	storeIds := sliceutils.CollectUniqueIDs(res, func(o *order.OrderSummary) string {
		if o.Store != nil {
			return o.Store.Id
		}
		return ""
	})

	stores, err := h.storeGW.GetByIds(ctx, storeIds)
	if err == nil {
		// Chỉ map nếu lấy được store, không thì trả về data gốc
		res = mapper.MapStoreToOrderHeaders(res, stores)
	}

	deliveryManIds := sliceutils.CollectUniqueIDs(res, func(o *order.OrderSummary) string {
		if o.DeliveryMan != nil {
			return o.DeliveryMan.Id
		}
		return ""
	})

	deliveryMans, err := h.userGW.GetByIds(ctx, deliveryManIds)
	if err == nil {
		res = mapper.MapDeliveryManToOrderHeaders(res, deliveryMans)
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// CreateOrder
// @Summary      Place New Order
// @Description  Create a new order based on the provided items and shipping information.
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body     order.CreateOrderRequest  true  "Order creation details"
// @Success      200     {object} map[string]interface{}    "Created Order Data"
// @Failure      400     {object} map[string]string         "Validation error"
// @Router       /orders [post]
func (h *OrderHandler) CreateOrder(c *gin.Context) error {
	var req order.CreateOrderRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	res, err := h.orderGW.CreateOrder(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// UpdateOrder
// @Summary      Update Order Items
// @Description  Update the list of items in an existing order (usually allowed only when order is in 'Pending' state).
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                    true  "Order UUID"
// @Param        request  body     []order.UpdateOrderItem   true  "List of items to update"
// @Success      200      {object} map[string]interface{}    "Updated Order Data"
// @Failure      400      {object} map[string]string         "Validation error"
// @Router       /orders/{id} [put]
func (h *OrderHandler) UpdateOrder(c *gin.Context) error {
	id := c.Param("id")
	orderId, err := uuid.Parse(id)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req []*order.UpdateOrderItem
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	res, err := h.orderGW.UpdateOrder(ctx, orderId, req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// UpdateOrderStatus
// @Summary      Update Order Status
// @Description  Change the status of an order (e.g., to 'Cancelled', 'Completed').
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                   true  "Order UUID"
// @Param        request  body     order.UpdateOrderStatus  true  "New Status"
// @Success      200      {object} map[string]interface{}   "Success"
// @Failure      400      {object} map[string]string        "Validation error"
// @Router       /orders/{id}/status [patch]
func (h *OrderHandler) UpdateOrderStatus(c *gin.Context) error {
	id := c.Param("id")
	orderId, err := uuid.Parse(id)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req order.UpdateOrderStatus
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	err = h.orderGW.UpdateStatus(ctx, orderId, req.Status)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}

// AssignStaffToDelivery
// @Summary      Assign Staff To Delivery
// @Description  Gán nhân viên vào ca làm để giao hàng
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                              true  "Order UUID"
// @Param        request  body     order.AssignStaffToDeliveryRequest  true  "Chứa staff id"
// @Success      200      {object} map[string]interface{}              "Success"
// @Failure      400      {object} map[string]string        		   "Validation error"
// @Router       /orders/{id}/delivery/assign [post]
func (h *OrderHandler) AssignStaffToDelivery(c *gin.Context) error {
	id := c.Param("id")
	orderId, err := uuid.Parse(id)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req order.AssignStaffToDeliveryRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	err = h.orderGW.AssignStaffToDelivery(ctx, orderId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}

// SelfAssignStaffToDelivery
// @Summary      Self Assign Staff To Delivery
// @Description  Tự gán bản thân vào ca làm để giao hàng
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                              	   true  "Order UUID"
// @Param        request  body     order.AssignStaffToDeliveryRequest      true  "Chứa staff id"
// @Success      200      {object} map[string]interface{}              	   "Success"
// @Failure      400      {object} map[string]string        		       "Validation error"
// @Router       /orders/{id}/delivery/me/assign [post]
func (h *OrderHandler) SelfAssignStaffToDelivery(c *gin.Context) error {
	id := c.Param("id")
	orderId, err := uuid.Parse(id)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req order.AssignStaffToDeliveryRequest
	if c.Request.Body != nil && c.Request.Body != http.NoBody {
		if err := c.ShouldBindJSON(&req); err != nil {
			return validation.TranslateValidationError(err)
		}
	}

	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return err
	}

	req.StaffId = &userId

	ctx := c.Request.Context()
	err = h.orderGW.AssignStaffToDelivery(ctx, orderId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}

// GetRevenueReport
// @Summary      Get Revenue Report (JSON)
// @Description  Get detailed revenue breakdown by product and category for charts.
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        query  query    order.GetRevenueReportRequest  false  "Filter params (fromDate, toDate, storeId, categoryId)"
// @Success      200    {object} order.GetRevenueReportReply    "Revenue data"
// @Router       /orders/reports/revenue [get]
func (h *OrderHandler) GetRevenueReport(c *gin.Context) error {
	var req order.GetRevenueReportRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	// Phân quyền: Manager/Staff chỉ được xem store của mình
	contextutils.ApplyStoreBoundaries(c, &req.StoreId)

	res, _, err := h.orderGW.GetRevenueReport(ctx, &req, false)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// ExportRevenueReport
// @Summary      Export Revenue Report (CSV)
// @Description  Download revenue report as a CSV file for bookkeeping.
// @Tags         Orders
// @Produce      text/csv
// @Security     BearerAuth
// @Param        query  query    order.GetRevenueReportRequest  false  "Filter params"
// @Success      200    {file}   binary                         "CSV File"
// @Router       /orders/reports/revenue/export [get]
func (h *OrderHandler) ExportRevenueReport(c *gin.Context) error {
	var req order.GetRevenueReportRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	contextutils.ApplyStoreBoundaries(c, &req.StoreId)

	_, csv, err := h.orderGW.GetRevenueReport(ctx, &req, true)
	if err != nil {
		return err
	}

	if csv == nil {
		return errorcode.ErrExportFailed
	}

	// Trả về file CSV cho trình duyệt tải về
	c.Header("Content-Disposition", "attachment; filename="+csv.CsvFileName)
	c.Data(http.StatusOK, "text/csv", csv.CsvRaw)
	return nil
}

// GetOrderVolume
// @Summary      Get Order Volume Statistics (JSON)
// @Description  Get daily order counts and status breakdown for line charts.
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        query  query    order.GetOrderVolumeRequest  false  "Filter params (fromDate, toDate, storeId)"
// @Success      200    {object} order.GetOrderVolumeReply    "Order volume data"
// @Router       /orders/reports/volume [get]
func (h *OrderHandler) GetOrderVolume(c *gin.Context) error {
	var req order.GetOrderVolumeRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	contextutils.ApplyStoreBoundaries(c, &req.StoreId)

	res, _, err := h.orderGW.GetOrderVolume(ctx, &req, false)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// ExportOrderVolume
// @Summary      Export Order Volume (CSV)
// @Description  Download daily order statistics as a CSV file.
// @Tags         Orders
// @Produce      text/csv
// @Security     BearerAuth
// @Param        query  query    order.GetOrderVolumeRequest  false  "Filter params"
// @Success      200    {file}   binary                       "CSV File"
// @Router       /orders/reports/volume/export [get]
func (h *OrderHandler) ExportOrderVolume(c *gin.Context) error {
	var req order.GetOrderVolumeRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	contextutils.ApplyStoreBoundaries(c, &req.StoreId)

	_, csv, err := h.orderGW.GetOrderVolume(ctx, &req, true)
	if err != nil {
		return err
	}

	if csv == nil {
		return errorcode.ErrExportFailed
	}

	c.Header("Content-Disposition", "attachment; filename="+csv.CsvFileName)
	c.Data(http.StatusOK, "text/csv", csv.CsvRaw)
	return nil
}

// GetSalesTrend
// @Summary      Lấy biểu đồ xu hướng doanh thu
// @Description  Lấy dữ liệu xu hướng doanh thu (doanh thu, lượng đơn hàng) trong một khoảng thời gian được gom nhóm (giờ/ngày/tuần).
// @Description  LƯU Ý VỀ PHÂN QUYỀN (StoreId):
// @Description  - Nếu là Admin: Có thể truyền tham số storeId để xem cụ thể một cửa hàng, hoặc không truyền (bỏ trống) để xem dữ liệu của toàn hệ thống.
// @Description  - Nếu là Manager/Staff: Truyền storeId hay không thì hệ thống cũng sẽ tự động ghi đè (override) bằng ID cửa hàng của tài khoản đang đăng nhập.
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        query  query     order.GetSalesTrendRequest  true  "Các tham số lọc (fromDate, toDate, aggregation, storeId...)"
// @Success      200    {object}  order.GetSalesTrendReply    "Dữ liệu xu hướng và nhãn thời gian cao điểm/thấp điểm"
// @Failure      400    {object}  map[string]string           "Lỗi dữ liệu đầu vào (Bad Request)"
// @Failure      401    {object}  map[string]string           "Lỗi xác thực (Unauthorized)"
// @Router       /orders/reports/sales-trend [get]
func (h *OrderHandler) GetSalesTrend(c *gin.Context) error {
	var req order.GetSalesTrendRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	contextutils.ApplyStoreBoundaries(c, &req.StoreId)

	res, _, err := h.orderGW.GetSalesTrend(ctx, &req, false)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// ExportSalesTrend
// @Summary      Xuất file CSV xu hướng doanh thu
// @Description  Tải xuống dữ liệu xu hướng doanh thu (doanh thu, lượng đơn hàng) dưới dạng file CSV.
// @Description  Hỗ trợ các tham số lọc và phân quyền storeId tương tự như API lấy JSON.
// @Tags         Orders
// @Produce      text/csv
// @Security     BearerAuth
// @Param        query  query    order.GetSalesTrendRequest  true  "Các tham số lọc (fromDate, toDate, aggregation, storeId...)"
// @Success      200    {file}   binary                       "Tệp CSV chứa dữ liệu xu hướng doanh thu"
// @Failure      400    {object} map[string]string            "Lỗi dữ liệu đầu vào"
// @Router       /orders/reports/sales-trend/export [get]
func (h *OrderHandler) ExportSalesTrend(c *gin.Context) error {
	var req order.GetSalesTrendRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	contextutils.ApplyStoreBoundaries(c, &req.StoreId)

	_, csv, err := h.orderGW.GetSalesTrend(ctx, &req, true)
	if err != nil {
		return err
	}

	if csv == nil {
		return errorcode.ErrExportFailed
	}

	c.Header("Content-Disposition", "attachment; filename="+csv.CsvFileName)
	c.Data(http.StatusOK, "text/csv", csv.CsvRaw)
	return nil
}
