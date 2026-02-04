using System.Text.Json.Serialization;

namespace BuildingBlocks.Core.Wrapper
{
    // 1. Dạng Void (Chỉ báo thành công, không có data)
    public class Result
    {
        // Luôn luôn là true vì nếu lỗi đã throw exception rồi
        public bool IsSuccess { get; } = true;

        // Constructor protected để ép dùng Static Method
        protected Result() { }

        // Hàm tạo nhanh
        public static Result Success() => new Result();
    }

    // 2. Dạng có Data (Wrapper)
    public class Result<T> : Result
    {
        public T Data { get; }

        // Constructor private
        private Result(T data) : base()
        {
            Data = data;
        }

        // Hàm tạo nhanh
        public static Result<T> Success(T data) => new Result<T>(data);

        public static implicit operator Result<T>(T data) => new Result<T>(data);
    }
}