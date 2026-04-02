using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopQuanAo.Data;

namespace ShopQuanAo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly ShopQuanAoContext _context;

        public HomeController(ShopQuanAoContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Tổng doanh thu (Chỉ tính những đơn hàng Đã Hoàn Thành - Trạng thái 3)
            var doanhThu = await _context.DonHangs
                .Where(d => d.TrangThaiDH == 3)
                .SumAsync(d => d.TongTien);

            // 2. Đếm số đơn hàng mới (Chờ xác nhận - Trạng thái 1)
            var donMoi = await _context.DonHangs
                .CountAsync(d => d.TrangThaiDH == 1);

            // 3. Đếm tổng số khách hàng đã đăng ký
            var soKhachHang = await _context.KhachHangs.CountAsync();

            // 4. Cảnh báo Hết hàng (Đếm số biến thể có số lượng tồn dưới 10 cái)
            var sapHetHang = await _context.BienTheSanPhams
                .CountAsync(b => b.SoLuongTon < 10);

            // 5. Chuẩn bị dữ liệu cho Biểu đồ (Thống kê số lượng theo 4 trạng thái)
            var thongKeDonHang = new List<int>
            {
                await _context.DonHangs.CountAsync(d => d.TrangThaiDH == 1), // Chờ xác nhận
                await _context.DonHangs.CountAsync(d => d.TrangThaiDH == 2), // Đang giao
                await _context.DonHangs.CountAsync(d => d.TrangThaiDH == 3), // Hoàn thành
                await _context.DonHangs.CountAsync(d => d.TrangThaiDH == 4)  // Đã hủy
            };

            // Truyền dữ liệu ra màn hình bằng ViewBag
            ViewBag.DoanhThu = doanhThu;
            ViewBag.DonMoi = donMoi;
            ViewBag.SoKhachHang = soKhachHang;
            ViewBag.SapHetHang = sapHetHang;
            ViewBag.ThongKeDonHang = thongKeDonHang;

            return View();
        }
    }
}