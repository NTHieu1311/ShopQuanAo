using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopQuanAo.Data;
using ShopQuanAo.Models;

namespace ShopQuanAo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DonHangController : Controller
    {
        private readonly ShopQuanAoContext _context;

        public DonHangController(ShopQuanAoContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. HIỂN THỊ DANH SÁCH ĐƠN HÀNG
        // ==========================================
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách đơn hàng, sắp xếp ngày đặt mới nhất lên đầu
            var donHangs = await _context.DonHangs
                .Include(d => d.TaiKhoan)
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();
            return View(donHangs);
        }

        // ==========================================
        // 2. XEM CHI TIẾT ĐƠN HÀNG
        // ==========================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var donHang = await _context.DonHangs
                .Include(d => d.TaiKhoan)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.BienTheSanPham)
                        .ThenInclude(b => b.SanPham)
                .FirstOrDefaultAsync(m => m.MaDH == id);

            if (donHang == null) return NotFound();

            return View(donHang);
        }

        // ==========================================
        // 3. XÁC NHẬN ĐƠN HÀNG (Trạng thái 1 -> 2)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhan(int id)
        {
            var donHang = await _context.DonHangs.FindAsync(id);

            // Chỉ xác nhận khi đơn hàng đang ở trạng thái 1 (Chờ xác nhận)
            if (donHang != null && donHang.TrangThaiDH == 1)
            {
                donHang.TrangThaiDH = 2; // Chuyển sang 2: Đang giao hàng
                _context.Update(donHang);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã xác nhận và chuyển giao đơn hàng #{id}.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 4. HOÀN THÀNH ĐƠN HÀNG (Trạng thái 2 -> 3)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HoanThanh(int id)
        {
            var donHang = await _context.DonHangs.FindAsync(id);

            // Chỉ cho phép hoàn thành khi đang ở trạng thái 2 (Đang giao)
            if (donHang != null && donHang.TrangThaiDH == 2)
            {
                donHang.TrangThaiDH = 3; // Hoàn thành
                _context.Update(donHang);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đơn hàng #{id} đã giao thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 5. ADMIN HỦY ĐƠN HÀNG (Và cộng lại Tồn kho)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyDon(int id)
        {
            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefaultAsync(d => d.MaDH == id);

            // Chỉ cho phép hủy nếu đơn chưa hoàn thành hoặc chưa bị hủy trước đó
            if (donHang != null && donHang.TrangThaiDH != 3 && donHang.TrangThaiDH != 4)
            {
                donHang.TrangThaiDH = 4; // 4: Đã hủy
                _context.Update(donHang);

                // Trả lại số lượng tồn kho cho Shop
                foreach (var ct in donHang.ChiTietDonHangs)
                {
                    var bienThe = await _context.BienTheSanPhams.FindAsync(ct.MaBienThe);
                    if (bienThe != null)
                    {
                        bienThe.SoLuongTon += ct.SoLuong; // Cộng trả lại số lượng
                        _context.Update(bienThe);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["ErrorMessage"] = $"Đã hủy đơn hàng #{id} và hoàn lại tồn kho!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}