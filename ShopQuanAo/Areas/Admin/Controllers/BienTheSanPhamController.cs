using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopQuanAo.Data;
using ShopQuanAo.Models;

namespace ShopQuanAo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BienTheSanPhamController : Controller
    {
        private readonly ShopQuanAoContext _context;

        public BienTheSanPhamController(ShopQuanAoContext context)
        {
            _context = context;
        }

        // 1. HIỂN THỊ DANH SÁCH
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách biến thể, kèm theo thông tin Sản Phẩm gốc
            var bienThes = await _context.BienTheSanPhams.Include(b => b.SanPham).ToListAsync();
            return View(bienThes);
        }

        // 2. THÊM MỚI
        public IActionResult Create()
        {
            // Dropdown chọn Sản phẩm (Chỉ hiển thị sản phẩm đang bán)
            ViewData["MaSP"] = new SelectList(_context.SanPhams.Where(s => s.TrangThai == 1), "MaSP", "TenSP");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BienTheSanPham bienTheSanPham)
        {
            // Bỏ qua kiểm tra các bảng liên kết
            ModelState.Remove("SanPham");
            ModelState.Remove("ChiTietDonHangs");
            ModelState.Remove("ChiTietGioHangs");

            if (ModelState.IsValid)
            {
                _context.Add(bienTheSanPham);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm biến thể thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaSP"] = new SelectList(_context.SanPhams, "MaSP", "TenSP", bienTheSanPham.MaSP);
            return View(bienTheSanPham);
        }

        // 3. CẬP NHẬT
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var bienThe = await _context.BienTheSanPhams.FindAsync(id);
            if (bienThe == null) return NotFound();

            ViewData["MaSP"] = new SelectList(_context.SanPhams, "MaSP", "TenSP", bienThe.MaSP);
            return View(bienThe);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BienTheSanPham bienTheSanPham)
        {
            if (id != bienTheSanPham.MaBienThe) return NotFound();

            ModelState.Remove("SanPham");
            ModelState.Remove("ChiTietDonHangs");
            ModelState.Remove("ChiTietGioHangs");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bienTheSanPham);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật biến thể thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BienTheExists(bienTheSanPham.MaBienThe)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaSP"] = new SelectList(_context.SanPhams, "MaSP", "TenSP", bienTheSanPham.MaSP);
            return View(bienTheSanPham);
        }

        // 4. XÓA
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var bienThe = await _context.BienTheSanPhams
                .Include(b => b.SanPham)
                .FirstOrDefaultAsync(m => m.MaBienThe == id);

            if (bienThe == null) return NotFound();

            return View(bienThe);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bienThe = await _context.BienTheSanPhams.FindAsync(id);
            if (bienThe != null)
            {
                try
                {
                    _context.BienTheSanPhams.Remove(bienThe);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã xóa biến thể thành công!";
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] = "Không thể xóa! Biến thể này đã nằm trong đơn hàng của khách.";
                    return RedirectToAction(nameof(Delete), new { id = id });
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BienTheExists(int id)
        {
            return _context.BienTheSanPhams.Any(e => e.MaBienThe == id);
        }
    }
}