using ShopQuanAo.Libraries;

namespace ShopQuanAo.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;

        public VnPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreatePaymentUrl(HttpContext context, int maDH, decimal tongTien, string noiDung)
        {
            var vnpay = new VnPayLibrary();
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);

            vnpay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            vnpay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            vnpay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            vnpay.AddRequestData("vnp_Amount", ((long)tongTien * 100).ToString());
            vnpay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            vnpay.AddRequestData("vnp_IpAddr", context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            vnpay.AddRequestData("vnp_OrderInfo", noiDung);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", $"{context.Request.Scheme}://{context.Request.Host}/ThanhToan/PaymentCallback");
            vnpay.AddRequestData("vnp_TxnRef", maDH.ToString()); // Mã tham chiếu của giao dịch (Chính là Mã Đơn Hàng)

            var paymentUrl = vnpay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);
            return paymentUrl;
        }
    }
}