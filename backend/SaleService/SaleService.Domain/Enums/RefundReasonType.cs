namespace SalesService.Domain.Enums
{
    public enum RefundReasonType

    {
        NormalProcess = 0, // không được dùng overrideInternalUser để mở thêm quyền refund
        //  Khách huỷ đơn hợp lệ

        //Đơn chưa pha / chưa giao

        //Hoàn tiền do sai sót nhỏ nhưng đúng policy


        SystemError = 1,
//        Thanh toán bị trừ tiền 2 lần

//Giao dịch thành công nhưng đơn không tạo

//Lỗi đồng bộ payment gateway


        CustomerComplaint = 2,
//        Đồ uống không đúng order

//Chất lượng kém

//Giao hàng trễ / sai món

        GoodwillGesture = 3,
//        Giữ chân khách VIP

//Xử lý tình huống nhạy cảm

//Tặng lại tiền như một ưu đãi

        Compensation = 4
//Giao nhầm đồ gây dị ứng

//Tai nạn nhỏ trong quá trình phục vụ

//Lỗi nghiêm trọng ảnh hưởng trải nghiệm

    }

}
