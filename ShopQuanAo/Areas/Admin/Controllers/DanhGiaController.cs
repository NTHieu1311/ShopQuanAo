using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopQuanAo.Data;
using ShopQuanAo.Models;

namespace ShopQuanAo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DanhGiaController : Controller
    {
        private readonly ShopQuanAoContext _context;

        public DanhGiaController(ShopQuanAoContext context)
        {
            _context = context;
        }

        // 1. HIỂN THỊ DANH SÁCH ĐÁNH GIÁ
        public async Task<IActionResult> Index()
        {
            // Kéo theo bảng SanPham và TaiKhoan để lấy Tên SP và Tên người đánh giá
            var danhGias = await _context.DanhGias
                .Include(d => d.SanPham)
                .Include(d => d.TaiKhoan)
                .OrderByDescending(d => d.NgayDanhGia)
                .ToListAsync();
            return View(danhGias);
        }

        // 2. CẬP NHẬT TRẠNG THÁI (ẨN/HIỆN)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var danhGia = await _context.DanhGias
                .Include(d => d.SanPham)
                .Include(d => d.TaiKhoan)
                .FirstOrDefaultAsync(d => d.MaDanhGia == id);

            if (danhGia == null) return NotFound();
            return View(danhGia);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Chỉ nhận ID và Trạng Thái từ form
        public async Task<IActionResult> Edit(int id, int TrangThai)
        {
            var danhGia = await _context.DanhGias.FindAsync(id);
            if (danhGia == null) return NotFound();

            try
            {
                danhGia.TrangThai = TrangThai;
                _context.Update(danhGia);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã cập nhật trạng thái hiển thị của đánh giá!";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Lỗi khi cập nhật!";
            }
            return RedirectToAction(nameof(Index));
        }

        // 3. XÓA ĐÁNH GIÁ (Nếu spam)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var danhGia = await _context.DanhGias
                .Include(d => d.SanPham)
                .Include(d => d.TaiKhoan)
                .FirstOrDefaultAsync(m => m.MaDanhGia == id);

            if (danhGia == null) return NotFound();
            return View(danhGia);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var danhGia = await _context.DanhGias.FindAsync(id);
            if (danhGia != null)
            {
                _context.DanhGias.Remove(danhGia);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa đánh giá thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}