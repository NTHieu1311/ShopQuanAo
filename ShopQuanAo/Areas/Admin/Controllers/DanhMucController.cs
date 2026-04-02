using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopQuanAo.Data;
using ShopQuanAo.Models;

namespace ShopQuanAo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DanhMucController : Controller
    {
        private readonly ShopQuanAoContext _context;

        public DanhMucController(ShopQuanAoContext context)
        {
            _context = context;
        }

        // 1. HIỂN THỊ DANH SÁCH
        public async Task<IActionResult> Index()
        {
            var danhMucs = await _context.DanhMucs.ToListAsync();
            return View(danhMucs);
        }

        // ==========================================
        // 2. THÊM MỚI (CREATE)
        // ==========================================
        // GET: Hiển thị form thêm mới
        public IActionResult Create()
        {
            return View();
        }

        // POST: Nhận dữ liệu từ form và lưu vào DB
        [HttpPost]
        [ValidateAntiForgeryToken] // Bảo mật chống fake request
        public async Task<IActionResult> Create(DanhMuc danhMuc)
        {
            if (ModelState.IsValid)
            {
                _context.Add(danhMuc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(danhMuc);
        }

        // ==========================================
        // 3. CẬP NHẬT (EDIT)
        // ==========================================
        // GET: Lấy dữ liệu cũ hiển thị lên form
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc == null) return NotFound();

            return View(danhMuc);
        }

        // POST: Lưu dữ liệu mới chỉnh sửa vào DB
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DanhMuc danhMuc)
        {
            if (id != danhMuc.MaDM) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(danhMuc);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DanhMucExists(danhMuc.MaDM)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(danhMuc);
        }

        // ==========================================
        // 4. XÓA (DELETE)
        // ==========================================
        // GET: Hiển thị trang xác nhận xóa
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var danhMuc = await _context.DanhMucs.FirstOrDefaultAsync(m => m.MaDM == id);
            if (danhMuc == null) return NotFound();

            return View(danhMuc);
        }

        // POST: Thực hiện xóa thật sự
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc != null)
            {
                try
                {
                    _context.DanhMucs.Remove(danhMuc);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã xóa danh mục thành công!";
                }
                catch (DbUpdateException)
                {
                    // Bắt lỗi ràng buộc khóa ngoại (Còn sản phẩm dính với danh mục)
                    TempData["ErrorMessage"] = "Không thể xóa! Danh mục này đang chứa sản phẩm. Vui lòng xóa hoặc đổi danh mục cho các sản phẩm đó trước.";
                    return RedirectToAction(nameof(Delete), new { id = id });
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // Hàm hỗ trợ kiểm tra tồn tại
        private bool DanhMucExists(int id)
        {
            return _context.DanhMucs.Any(e => e.MaDM == id);
        }
    }
}