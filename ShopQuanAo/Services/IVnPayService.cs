using Microsoft.AspNetCore.Http;

namespace ShopQuanAo.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(HttpContext context, int maDH, decimal tongTien, string noiDung);
    }
}