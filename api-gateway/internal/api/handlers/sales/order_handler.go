package sales

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/application/gateways/sales"
	"api-gateway/internal/domain/models/order"
	"api-gateway/pkg/mapper"
	"api-gateway/pkg/utils/contextutils"
	"api-gateway/pkg/utils/grpcutils"
	"api-gateway/pkg/utils/sliceutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type OrderHandler struct {
	orderGW sales.IOrderService
	storeGW franchise.IStoreService
}

func NewOrderHandler(
	orderGW sales.IOrderService,
	storeGW franchise.IStoreService,
) *OrderHandler {
	return &OrderHandler{
		orderGW: orderGW,
		storeGW: storeGW,
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

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

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

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

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

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

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

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

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

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

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

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	err = h.orderGW.UpdateStatus(ctx, orderId, req.Status)
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
<<<<<<< HEAD
<<<<<<< HEAD
// @Param        id       path     string                    true  "Order UUID"
// @Param        request  body     order.CreatePaymentRequest true "Payment details"
// @Success      200      {object} map[string]interface{}    "Payment URL"
// @Failure      400      {object} map[string]string         "Validation error"
// @Router       /orders/{id}/payment [post]
func (h *OrderHandler) CreatePaymentUrl(c *gin.Context) error {
	var req order.CreatePaymentRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	id := c.Param("id")
	orderId, err := uuid.Parse(id)
	if err != nil {
		return errorcode.ErrInvalidId
=======
// @Param        query  query    order.GetRevenueReportRequest  false  "Filter params (fromDate, toDate, storeId, categoryId)"
// @Success      200    {object} order.GetRevenueReportReply    "Revenue data"
// @Router       /orders/reports/revenue [get]
func (h *OrderHandler) GetRevenueReport(c *gin.Context) error {
	var req order.GetRevenueReportRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
>>>>>>> 67995ea2d691e474b75c2df4113ec3b4637af960
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	// Phân quyền: Manager/Staff chỉ được xem store của mình
	contextutils.ApplyStoreBoundaries(c, &req.StoreId)

	res, _, err := h.orderGW.GetRevenueReport(ctx, &req, false)
	if err != nil {
		return err
	}

<<<<<<< HEAD
	c.JSON(http.StatusOK, gin.H{
		"paymentUrl": res,
	})
	return nil
}

// GetRevenueReport
// @Summary      Get Revenue Report (JSON)
// @Description  Get detailed revenue breakdown by product and category for charts.
// @Tags         Orders
// @Accept       json
// @Produce      json
// @Security     BearerAuth
=======
>>>>>>> 67a394a75859432ebb4e70c583813a2d5a7dff05
// @Param        query  query    order.GetRevenueReportRequest  false  "Filter params (fromDate, toDate, storeId, categoryId)"
// @Success      200    {object} order.GetRevenueReportReply    "Revenue data"
// @Router       /orders/reports/revenue [get]
func (h *OrderHandler) GetRevenueReport(c *gin.Context) error {
	var req order.GetRevenueReportRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

	// Phân quyền: Manager/Staff chỉ được xem store của mình
	contextutils.ApplyStoreBoundaries(c, &req.StoreId)

	res, _, err := h.orderGW.GetRevenueReport(ctx, &req, false)
	if err != nil {
		return err
	}

<<<<<<< HEAD
=======
>>>>>>> 67995ea2d691e474b75c2df4113ec3b4637af960
=======
>>>>>>> 67a394a75859432ebb4e70c583813a2d5a7dff05
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

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

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

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

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

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		return err
	}

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
