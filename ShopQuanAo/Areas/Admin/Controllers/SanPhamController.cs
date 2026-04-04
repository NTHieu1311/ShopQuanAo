using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopQuanAo.Data;
using ShopQuanAo.Models;
using ShopQuanAo.Helpers; // [ĐÃ THÊM]: Dòng này để gọi được thư viện Helper

namespace ShopQuanAo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SanPhamController : Controller
    {
        private readonly ShopQuanAoContext _context;
        // [ĐÃ SỬA]: Xóa IWebHostEnvironment vì ta không lưu ảnh vào máy tính nữa
        // [ĐÃ THÊM]: Khai báo CloudinaryHelper
        private readonly CloudinaryHelper _cloudinaryHelper;

        public SanPhamController(ShopQuanAoContext context, CloudinaryHelper cloudinaryHelper)
        {
            _context = context;
            _cloudinaryHelper = cloudinaryHelper;
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
            ViewBag.ListDanhMuc = new SelectList(_context.DanhMucs.Where(d => d.TrangThai == 1), "MaDM", "TenDM");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SanPham sanPham, IFormFile? HinhAnh)
        {
            ModelState.Remove("DanhMuc");
            ModelState.Remove("BienTheSanPhams");
            ModelState.Remove("DanhGias");

            if (ModelState.IsValid)
            {
                // [ĐÃ SỬA]: Gọi CloudinaryHelper để đẩy ảnh lên mây
                if (HinhAnh != null && HinhAnh.Length > 0)
                {
                    // Hàm UploadImageAsync sẽ tự động làm hết việc và trả về cái Link
                    string linkAnhCloud = await _cloudinaryHelper.UploadImageAsync(HinhAnh, "FashionStore/SanPhams");

                    if (!string.IsNullOrEmpty(linkAnhCloud))
                    {
                        sanPham.HinhAnhChinh = linkAnhCloud; // Lưu đường link bắt đầu bằng https://res.cloudinary.com/...
                    }
                }
                else
                {
                    sanPham.HinhAnhChinh = ""; // Nếu không upload ảnh thì để rỗng hoặc link ảnh mặc định
                }

                sanPham.NgayTao = DateTime.Now;

                _context.Add(sanPham);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ListDanhMuc = new SelectList(_context.DanhMucs.Where(d => d.TrangThai == 1), "MaDM", "TenDM", sanPham.MaDM);
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

            ViewBag.ListDanhMuc = new SelectList(_context.DanhMucs.Where(d => d.TrangThai == 1), "MaDM", "TenDM", sanPham.MaDM);
            return View(sanPham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SanPham sanPham, IFormFile? HinhAnh)
        {
            if (id != sanPham.MaSP) return NotFound();

            ModelState.Remove("DanhMuc");
            ModelState.Remove("BienTheSanPhams");
            ModelState.Remove("DanhGias");

            if (ModelState.IsValid)
            {
                try
                {
                    // [ĐÃ SỬA]: Cập nhật ảnh lên Cloudinary
                    if (HinhAnh != null && HinhAnh.Length > 0)
                    {
                        // Đẩy ảnh mới lên mây
                        string linkAnhCloud = await _cloudinaryHelper.UploadImageAsync(HinhAnh, "FashionStore/SanPhams");

                        if (!string.IsNullOrEmpty(linkAnhCloud))
                        {
                            // Lưu link ảnh mới vào DB (Chúng ta tạm thời bỏ qua việc xóa ảnh cũ trên Cloudinary để đơn giản hóa code)
                            sanPham.HinhAnhChinh = linkAnhCloud;
                        }
                    }
                    // Nếu HinhAnh == null (Người dùng không chọn ảnh mới), Entity Framework sẽ tự động giữ nguyên link ảnh cũ trong DB

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

            ViewBag.ListDanhMuc = new SelectList(_context.DanhMucs.Where(d => d.TrangThai == 1), "MaDM", "TenDM", sanPham.MaDM);
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
                    _context.SanPhams.Remove(sanPham);
                    await _context.SaveChangesAsync();

                    // [ĐÃ SỬA]: Xóa đoạn code xóa ảnh trên ổ cứng vì ảnh giờ đã nằm trên mây
                    // (Bạn có thể tìm hiểu thêm về cách dùng API của Cloudinary để xóa ảnh trên đó sau nếu muốn)

                    TempData["SuccessMessage"] = "Đã xóa sản phẩm thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
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