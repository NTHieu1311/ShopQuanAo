using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopQuanAo.Data;
using ShopQuanAo.Models;

namespace ShopQuanAo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SanPhamController : Controller
    {
        private readonly ShopQuanAoContext _context;
        private readonly IWebHostEnvironment _env;

        // Bơm cả Context và Environment vào
        public SanPhamController(ShopQuanAoContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ==========================================
        // 1. HIỂN THỊ DANH SÁCH
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var sanPhams = await _context.SanPhams.Include(s => s.DanhMuc).ToListAsync();
            return View(sanPhams);
        }

        // ==========================================
        // 2. THÊM MỚI (CREATE)
        // ==========================================
        public IActionResult Create()
        {
            ViewData["MaDM"] = new SelectList(_context.DanhMucs.Where(d => d.TrangThai == 1), "MaDM", "TenDM");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SanPham sanPham, IFormFile HinhAnh)
        {
            if (ModelState.IsValid)
            {
                if (HinhAnh != null && HinhAnh.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "sanpham");

                    // Tạo thư mục nếu chưa tồn tại
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + HinhAnh.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await HinhAnh.CopyToAsync(fileStream);
                    }

                    sanPham.HinhAnhChinh = "/uploads/sanpham/" + uniqueFileName;
                }
                else
                {
                    sanPham.HinhAnhChinh = "";
                }

                sanPham.NgayTao = DateTime.Now;

                _context.Add(sanPham);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["MaDM"] = new SelectList(_context.DanhMucs, "MaDM", "TenDM", sanPham.MaDM);
            return View(sanPham);
        }

        // ==========================================
        // 3. CẬP NHẬT (EDIT)
        // ==========================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham == null) return NotFound();

            ViewData["MaDM"] = new SelectList(_context.DanhMucs.Where(d => d.TrangThai == 1), "MaDM", "TenDM", sanPham.MaDM);
            return View(sanPham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SanPham sanPham, IFormFile? HinhAnh)
        {
            if (id != sanPham.MaSP) return NotFound();

            // Bỏ qua kiểm tra các Navigation Properties
            ModelState.Remove("DanhMuc");
            ModelState.Remove("BienTheSanPhams");
            ModelState.Remove("DanhGias");

            if (ModelState.IsValid)
            {
                try
                {
                    if (HinhAnh != null && HinhAnh.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "sanpham");

                        // Đã bổ sung tạo thư mục tránh lỗi DirectoryNotFoundException
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + HinhAnh.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await HinhAnh.CopyToAsync(fileStream);
                        }

                        if (!string.IsNullOrEmpty(sanPham.HinhAnhChinh))
                        {
                            string oldFilePath = Path.Combine(_env.WebRootPath, sanPham.HinhAnhChinh.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        sanPham.HinhAnhChinh = "/uploads/sanpham/" + uniqueFileName;
                    }

                    _context.Update(sanPham);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SanPhamExists(sanPham.MaSP)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["MaDM"] = new SelectList(_context.DanhMucs, "MaDM", "TenDM", sanPham.MaDM);
            return View(sanPham);
        }

        // ==========================================
        // 4. XÓA (DELETE)
        // ==========================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var sanPham = await _context.SanPhams
                .Include(s => s.DanhMuc)
                .FirstOrDefaultAsync(m => m.MaSP == id);

            if (sanPham == null) return NotFound();

            return View(sanPham);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham != null)
            {
                try
                {
                    // 1. Thử xóa dữ liệu trong Database trước
                    _context.SanPhams.Remove(sanPham);
                    await _context.SaveChangesAsync();

                    // 2. Nếu DB xóa thành công, thì mới xóa file ảnh vật lý trên server
                    if (!string.IsNullOrEmpty(sanPham.HinhAnhChinh))
                    {
                        string filePath = Path.Combine(_env.WebRootPath, sanPham.HinhAnhChinh.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }

                    TempData["SuccessMessage"] = "Đã xóa sản phẩm và hình ảnh thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    // Bắt lỗi khi Sản phẩm đang được liên kết với bảng khác
                    TempData["ErrorMessage"] = "Không thể xóa! Sản phẩm này đang có biến thể, đánh giá hoặc nằm trong đơn hàng. Vui lòng xóa các dữ liệu liên quan trước.";
                    return RedirectToAction(nameof(Delete), new { id = id });
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private bool SanPhamExists(int id)
        {
            return _context.SanPhams.Any(e => e.MaSP == id);
        }
    }
}