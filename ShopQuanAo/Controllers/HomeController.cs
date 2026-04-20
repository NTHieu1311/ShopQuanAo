using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopQuanAo.Data;
using ShopQuanAo.Filters;
using ShopQuanAo.Models;
using System.Diagnostics;

namespace ShopQuanAo.Controllers
{
    [BlockAdmin]
    public class HomeController : Controller
    {
        private readonly ShopQuanAoContext _context;

        public HomeController(ShopQuanAoContext context)
        {
            _context = context;
        }

        
        // TRANG HỆ THỐNG CỬA HÀNG
        
        public IActionResult HeThongCuaHang()
        {
            return View();
        }

        
        // TRANG CHỦ (ĐÃ TỐI ƯU SIÊU TỐC ĐỘ)
        
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<IActionResult> Index()
        {
           
            var sanPhamMoi = await _context.SanPhams
                .AsNoTracking()
                .Include(s => s.DanhMuc)
                .Where(s => s.TrangThai == 1)
                .OrderByDescending(s => s.NgayTao)
                .Take(8)
                .ToListAsync();


            ViewBag.DanhMucs = await _context.DanhMucs
                .AsNoTracking()
                .Where(d => d.TrangThai == 1)
                .ToListAsync();

            return View(sanPhamMoi);
        }

        
        // TRANG CHÍNH SÁCH BẢO MẬT
        
        public IActionResult Privacy()
        {
            return View();
        }

        
        // TRANG BÁO LỖI
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}