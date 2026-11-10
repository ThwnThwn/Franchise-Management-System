package sales

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/application/gateways/identity"
	"api-gateway/internal/application/gateways/sales"
	_ "api-gateway/internal/domain/models/common"
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
// @Description  Retrieve a paginated list of all orders with optional filtering parameters (Admin/Manager view).
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        query  query     order.GetOrdersRequest  false  "Filter parameters (page, status, storeId, etc.)"
// @Success      200    {object}  common.PaginationResult[order.OrderSummary] "Paginated list of orders"
// @Failure      400    {object}  object "Validation Error (Bad Request)"
// @Failure      401    {object}  object "Unauthorized"
// @Failure      500    {object}  object "Internal Server Error"
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
// @Description  Retrieve full details of a specific order by its ID, including items, store info, and delivery personnel.
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id   path      string  true  "Order UUID"
// @Success      200  {object}  order.Order "Detailed order information"
// @Failure      400  {object}  object "Invalid ID format"
// @Failure      401  {object}  object "Unauthorized"
// @Failure      404  {object}  object "Order not found"
// @Failure      500  {object}  object "Internal Server Error"
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
// @Description  Retrieve a list of orders placed by the current logged-in customer.
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        query  query     order.GetMyOrdersRequest  false  "Filter parameters (page, status, etc.)"
// @Success      200    {array}   order.OrderSummary    "Llist of user's orders"
// @Failure      400    {object}  object "Validation Error (Bad Request)"
// @Failure      401    {object}  object "Unauthorized"
// @Failure      500    {object}  object "Internal Server Error"
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
// @Description  Create a new order based on the provided items, store, and shipping information.
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body     order.CreateOrderRequest  true  "Order creation details"
// @Success      200     {object} order.OrderReply    "Created Order Data"
// @Failure      400     {object} object "Validation Error (Bad Request)"
// @Failure      401     {object} object "Unauthorized"
// @Failure      500     {object} object "Internal Server Error"
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
// @Description  Update the list of items in an existing order (typically allowed only when the order is in a 'Pending' state).
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                   true  "Order UUID"
// @Param        request  body     []order.UpdateOrderItem  true  "List of items to update"
// @Success      200      {object} order.OrderReply   "Updated Order Data"
// @Failure      400      {object} object "Validation Error or Invalid ID"
// @Failure      401      {object} object "Unauthorized"
// @Failure      404      {object} object "Order Not Found"
// @Failure      500      {object} object "Internal Server Error"
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
// @Description  Change the lifecycle status of an order (e.g., to 'Processing', 'Cancelled', 'Completed').
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                   true  "Order UUID"
// @Param        request  body     order.UpdateOrderStatus  true  "New Status details"
// @Success      200      {object} map[string]interface{}   "Successfully updated status"
// @Failure      400      {object} object "Validation Error or Invalid ID"
// @Failure      401      {object} object "Unauthorized"
// @Failure      404      {object} object "Order Not Found"
// @Failure      500      {object} object "Internal Server Error"
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
// @Description  Manager assigns a specific staff member (delivery man) to handle an order's delivery.
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                              true  "Order UUID"
// @Param        request  body     order.AssignStaffToDeliveryRequest  true  "Staff ID to be assigned"
// @Success      200      {object} map[string]interface{}              "Successfully assigned staff"
// @Failure      400      {object} object "Validation Error or Invalid ID"
// @Failure      401      {object} object "Unauthorized"
// @Failure      404      {object} object "Order or Staff Not Found"
// @Failure      500      {object} object "Internal Server Error"
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
// @Description  A delivery staff member assigns themselves to handle an order's delivery. Staff ID is automatically extracted from the Token.
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                              true  "Order UUID"
// @Param        request  body     order.AssignStaffToDeliveryRequest  true  "Optional assignment notes"
// @Success      200      {object} map[string]interface{}              "Successfully self-assigned"
// @Failure      400      {object} object "Validation Error or Invalid ID"
// @Failure      401      {object} object "Unauthorized"
// @Failure      404      {object} object "Order Not Found"
// @Failure      500      {object} object "Internal Server Error"
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
// @Description  Get a detailed revenue breakdown by product and category, primarily used for rendering dashboard charts.
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        query  query     order.GetRevenueReportRequest  false  "Filter parameters (fromDate, toDate, storeId, categoryId)"
// @Success      200    {object}  order.GetRevenueReportReply    "Revenue report data"
// @Failure      400    {object}  object "Validation Error (Bad Request)"
// @Failure      401    {object}  object "Unauthorized"
// @Failure      500    {object}  object "Internal Server Error"
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
// @Description  Download the revenue report as a CSV file for bookkeeping and offline analysis.
// @Tags         Orders
// @Produce      text/csv
// @Security     BearerAuth
// @Param        query  query     order.GetRevenueReportRequest  false  "Filter parameters (fromDate, toDate, storeId, categoryId)"
// @Success      200    {file}    binary                         "CSV File containing revenue data"
// @Failure      400    {object}  object "Validation Error (Bad Request)"
// @Failure      401    {object}  object "Unauthorized"
// @Failure      500    {object}  object "Internal Server Error"
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
// @Description  Get daily order counts and status breakdown, primarily used for rendering line charts on the dashboard.
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        query  query     order.GetOrderVolumeRequest  false  "Filter parameters (fromDate, toDate, storeId)"
// @Success      200    {object}  order.GetOrderVolumeReply    "Order volume statistics data"
// @Failure      400    {object}  object "Validation Error (Bad Request)"
// @Failure      401    {object}  object "Unauthorized"
// @Failure      500    {object}  object "Internal Server Error"
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
// @Param        query  query     order.GetOrderVolumeRequest  false  "Filter parameters (fromDate, toDate, storeId)"
// @Success      200    {file}    binary                       "CSV File containing order volume data"
// @Failure      400    {object}  object "Validation Error (Bad Request)"
// @Failure      401    {object}  object "Unauthorized"
// @Failure      500    {object}  object "Internal Server Error"
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
// @Summary      Get Sales Trend Chart Data
// @Description  Retrieve sales trend data (revenue, order volume) grouped by a specific time aggregation (hour/day/week).
// @Description  **AUTHORIZATION NOTE (StoreId):**
// @Description  - **Admin:** Can provide a `storeId` to view a specific store, or leave it blank to view system-wide data.
// @Description  - **Manager/Staff:** Even if a `storeId` is provided, the system will securely override it with the store ID associated with the logged-in user's token.
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        query  query     order.GetSalesTrendRequest  true  "Filtering and grouping parameters (fromDate, toDate, aggregation, storeId...)"
// @Success      200    {object}  order.GetSalesTrendReply    "Trend data including peak/low time labels"
// @Failure      400    {object}  object "Validation Error (Bad Request)"
// @Failure      401    {object}  object "Unauthorized"
// @Failure      500    {object}  object "Internal Server Error"
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
// @Summary      Export Sales Trend (CSV)
// @Description  Download sales trend data (revenue, order volume) as a CSV file.
// @Description  Supports the same filtering and storeId authorization logic as the JSON endpoint.
// @Tags         Orders
// @Produce      text/csv
// @Security     BearerAuth
// @Param        query  query     order.GetSalesTrendRequest  true  "Filtering and grouping parameters (fromDate, toDate, aggregation, storeId...)"
// @Success      200    {file}    binary                      "CSV File containing sales trend data"
// @Failure      400    {object}  object "Validation Error (Bad Request)"
// @Failure      401    {object}  object "Unauthorized"
// @Failure      500    {object}  object "Internal Server Error"
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
