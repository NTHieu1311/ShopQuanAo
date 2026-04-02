using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopQuanAo.Data;
using ShopQuanAo.Models;

namespace ShopQuanAo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PhuongThucThanhToanController : Controller
    {
        private readonly ShopQuanAoContext _context;

        public PhuongThucThanhToanController(ShopQuanAoContext context)
        {
            _context = context;
        }

        // 1. HIỂN THỊ DANH SÁCH
        public async Task<IActionResult> Index()
        {
            return View(await _context.PhuongThucThanhToans.ToListAsync());
        }

        // 2. THÊM MỚI
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PhuongThucThanhToan phuongThuc)
        {
            // Bỏ qua kiểm tra danh sách Giao dịch liên quan
            ModelState.Remove("GiaoDichThanhToans");

            if (ModelState.IsValid)
            {
                _context.Add(phuongThuc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm phương thức thanh toán thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(phuongThuc);
        }

        // 3. CẬP NHẬT
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var phuongThuc = await _context.PhuongThucThanhToans.FindAsync(id);
            if (phuongThuc == null) return NotFound();
            return View(phuongThuc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PhuongThucThanhToan phuongThuc)
        {
            if (id != phuongThuc.MaPT) return NotFound();

            ModelState.Remove("GiaoDichThanhToans");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(phuongThuc);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhuongThucExists(phuongThuc.MaPT)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(phuongThuc);
        }

        // 4. XÓA
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var phuongThuc = await _context.PhuongThucThanhToans.FirstOrDefaultAsync(m => m.MaPT == id);
            if (phuongThuc == null) return NotFound();

            return View(phuongThuc);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phuongThuc = await _context.PhuongThucThanhToans.FindAsync(id);
            if (phuongThuc != null)
            {
                try
                {
                    _context.PhuongThucThanhToans.Remove(phuongThuc);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã xóa phương thức thanh toán!";
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] = "Không thể xóa do đang có giao dịch sử dụng phương thức này!";
                    return RedirectToAction(nameof(Delete), new { id = id });
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PhuongThucExists(int id)
        {
            return _context.PhuongThucThanhToans.Any(e => e.MaPT == id);
        }
    }
}