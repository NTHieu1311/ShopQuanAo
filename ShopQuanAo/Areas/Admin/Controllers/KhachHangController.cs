using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopQuanAo.Data;
using ShopQuanAo.Models;
using X.PagedList; 

namespace ShopQuanAo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class KhachHangController : Controller
    {
        private readonly ShopQuanAoContext _context;

        public KhachHangController(ShopQuanAoContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchKeyword, int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 50;

            var query = _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(searchKeyword))
            {
                string key = searchKeyword.Trim().ToLower();
                query = query.Where(k =>
                    k.HoTen.ToLower().Contains(key) ||
                    k.SoDienThoai.Contains(key) ||
                    k.TaiKhoan.Email.ToLower().Contains(key)
                );
            }

            query = query.OrderByDescending(k => k.MaTK);

            int totalItemCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var pagedData = new StaticPagedList<KhachHang>(items, pageNumber, pageSize, totalItemCount);

            ViewBag.SearchKeyword = searchKeyword;

            return View(pagedData);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var khachHang = await _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .FirstOrDefaultAsync(k => k.MaTK == id);

            if (khachHang == null) return NotFound();
            return View(khachHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int DiemTichLuy, int TrangThai)
        {
            var khachHang = await _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .FirstOrDefaultAsync(k => k.MaTK == id);

            if (khachHang == null) return NotFound();

            try
            {
                khachHang.DiemTichLuy = DiemTichLuy;
                khachHang.TaiKhoan.TrangThai = TrangThai;

                _context.Update(khachHang);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thông tin khách hàng thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu dữ liệu!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}