namespace SharedKernel.Constants
{
    /// <summary>
    /// Hang so thoi gian dung chung cho toan he thong.
    /// </summary>
    public static class TimeConstants
    {
        /// <summary>
        /// So phut toi da cho phep check-in truoc gio bat dau ca.
        /// </summary>
        public const int MaxEarlyCheckInMinutes = 30;

        /// <summary>
        /// So gio toi da cho phep check-out sau gio ket thuc ca.
        /// Neu vuot qua moc nay, he thong gioi han lai de tranh tinh sai.
        /// </summary>
        public const int MaxLateCheckOutHours = 4;
    }
}
