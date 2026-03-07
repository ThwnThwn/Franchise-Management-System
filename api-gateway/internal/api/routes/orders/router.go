package orders

import (
	"api-gateway/internal/api"

	"github.com/gin-gonic/gin"
)

func (o *OrderRouter) Init(r *gin.RouterGroup) {
	publicOrderApi := r.Group("/orders")
	{
		publicOrderApi.GET("/vnpay/return", api.WrapHandler(o.paymentHl.ProcessVnPayReturn))
	}

	orderApi := r.Group("/orders")
	orderApi.Use(o.authMW.ValidateToken([]byte(o.cfg.JwtSecret)))
	{
		// Nhóm Report (Nên đưa lên đầu và có thể thêm MW check Role)
		// Chỉ Admin/Manager/Staff mới được vào đây
		reportApi := orderApi.Group("/reports")
		{
			reportApi.GET("/revenue", o.authMW.PermissionCheck("order.report.read"),
				api.WrapHandler(o.orderHl.GetRevenueReport))

			reportApi.GET("/volume", o.authMW.PermissionCheck("order.report.read"),
				api.WrapHandler(o.orderHl.GetOrderVolume))

			reportApi.GET("/revenue/export", o.authMW.PermissionCheck("order.report.read"),
				api.WrapHandler(o.orderHl.ExportRevenueReport))

			reportApi.GET("/volume/export", o.authMW.PermissionCheck("order.report.read"),
				api.WrapHandler(o.orderHl.ExportOrderVolume))

			reportApi.GET("/sales-trend", o.authMW.PermissionCheck("order.report.read"),
				api.WrapHandler(o.orderHl.GetSalesTrend))

			reportApi.GET("/sales-trend/export", o.authMW.PermissionCheck("order.report.read"),
				api.WrapHandler(o.orderHl.ExportSalesTrend))
		}

		// Nhóm CRUD thông thường (Khai báo Static trước)
		orderApi.GET("", o.authMW.PermissionCheck("order.read"),
			api.WrapHandler(o.orderHl.GetOrders))
		orderApi.POST("", o.authMW.PermissionCheck("order.create"),
			api.WrapHandler(o.orderHl.CreateOrder))

		//  Nhóm có tham số ID (Khai báo cuối cùng)
		orderApi.GET("/:id", o.authMW.PermissionCheck("order.read"),
			api.WrapHandler(o.orderHl.GetOrderById))
		orderApi.PUT("/:id", o.authMW.PermissionCheck("order.update"),
			api.WrapHandler(o.orderHl.UpdateOrder))
		orderApi.PATCH("/:id/status", o.authMW.PermissionCheck("order.update"),
			api.WrapHandler(o.orderHl.UpdateOrderStatus))

		orderApi.POST("/:id/payment", o.authMW.PermissionCheck("order.payment"),
			api.WrapHandler(o.paymentHl.CreatePaymentUrl))
	}

	payments := r.Group("/orders/payments")
	payments.Use(o.authMW.ValidateToken([]byte(o.cfg.JwtSecret)))
	{
		// 1. Dành cho Admin/Kế toán: Xem "tất cả"
		payments.GET("", o.authMW.PermissionCheck("payment.read.all"), api.WrapHandler(o.paymentHl.GetAllPayments))
	}

	// 👤 Nhóm dành cho khách hàng tự xem đơn của mình
	myOrderApi := r.Group("/me/orders")
	myOrderApi.Use(o.authMW.ValidateToken([]byte(o.cfg.JwtSecret)))
	{
		myOrderApi.GET("", o.authMW.PermissionCheck("order.read"),
			api.WrapHandler(o.orderHl.GetMyOrders))

		myOrderApi.GET("/payments", o.authMW.PermissionCheck("payment.read"), api.WrapHandler(o.paymentHl.GetMyPaymentHistory))
	}
}
