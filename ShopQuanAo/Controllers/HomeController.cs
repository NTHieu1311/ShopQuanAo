using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopQuanAo.Data;
using ShopQuanAo.Models;
using System.Diagnostics;

namespace ShopQuanAo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ShopQuanAoContext _context;

        public HomeController(ShopQuanAoContext context)
        {
            _context = context;
        }
        // ==========================================
        // TRANG HỆ THỐNG CỬA HÀNG
        // ==========================================
        public IActionResult HeThongCuaHang()
        {
            return View();
        }
        public async Task<IActionResult> Index()
        {
            // 1. Lấy 8 sản phẩm mới nhất đang được bán
            var sanPhamMoi = await _context.SanPhams
                .Include(s => s.DanhMuc)
                .Where(s => s.TrangThai == 1)
                .OrderByDescending(s => s.NgayTao)
                .Take(8)
                .ToListAsync();

            // 2. Lấy danh sách Danh Mục để hiển thị ra Menu/Lưới
            ViewBag.DanhMucs = await _context.DanhMucs
                .Where(d => d.TrangThai == 1)
                .ToListAsync();

            // Truyền danh sách sản phẩm vào View
            return View(sanPhamMoi);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}