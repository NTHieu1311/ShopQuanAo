using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopQuanAo.Data;
using ShopQuanAo.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting; // Bổ sung thư viện
using Microsoft.AspNetCore.Http;    // Bổ sung thư viện
using System.IO;                    // Bổ sung thư viện

namespace ShopQuanAo.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly ShopQuanAoContext _context;
        private readonly IWebHostEnvironment _env; // Thêm biến môi trường để lưu file

        // Cập nhật Constructor để tiêm (inject) IWebHostEnvironment
        public SanPhamController(ShopQuanAoContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ==========================================
        // 1. HIỂN THỊ DANH SÁCH SẢN PHẨM (CÓ LỌC & PHÂN TRANG)
        // ==========================================
        public async Task<IActionResult> Index(string category, string keyword, decimal? minPrice, decimal? maxPrice, string color, string size, int page = 1)
        {
            var query = _context.SanPhams
                .Include(s => s.DanhMuc)
                .Include(s => s.BienTheSanPhams)
                .Where(s => s.TrangThai == 1)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(s => s.TenSP.Contains(keyword) || (s.DanhMuc != null && s.DanhMuc.TenDM.Contains(keyword)));
                ViewData["Title"] = $"Kết quả tìm kiếm: '{keyword}'";
            }
            else
            {
                ViewData["Title"] = string.IsNullOrEmpty(category) ? "Tất cả sản phẩm" : $"Thời trang {category}";
            }

            if (!string.IsNullOrEmpty(category)) query = query.Where(s => s.DanhMuc != null && s.DanhMuc.TenDM == category);
            if (minPrice.HasValue) query = query.Where(s => s.GiaBan >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(s => s.GiaBan <= maxPrice.Value);
            if (!string.IsNullOrEmpty(color)) query = query.Where(s => s.BienTheSanPhams.Any(b => b.MauSac == color && b.SoLuongTon > 0));
            if (!string.IsNullOrEmpty(size)) query = query.Where(s => s.BienTheSanPhams.Any(b => b.KichThuoc == size && b.SoLuongTon > 0));

            int pageSize = 9;
            int totalItems = await query.CountAsync();
            int totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 1;

            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var sanPhams = await query
                .OrderByDescending(s => s.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.Keyword = keyword;
            ViewBag.Category = category;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Color = color;
            ViewBag.Size = size;

            ViewBag.DanhMucs = await _context.DanhMucs.Where(d => d.TrangThai == 1).ToListAsync();
            ViewBag.CurrentCategory = category;
            ViewBag.AllColors = await _context.BienTheSanPhams.Select(b => b.MauSac).Distinct().ToListAsync();
            ViewBag.AllSizes = await _context.BienTheSanPhams.Select(b => b.KichThuoc).Distinct().ToListAsync();

            return View(sanPhams);
        }

        // ==========================================
        // 2. TRANG CHI TIẾT SẢN PHẨM
        // ==========================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var sanPham = await _context.SanPhams
                .Include(s => s.DanhMuc)
                .Include(s => s.BienTheSanPhams)
                .Include(s => s.DanhGias.Where(dg => dg.TrangThai == 1))
                    .ThenInclude(dg => dg.TaiKhoan)
                .FirstOrDefaultAsync(m => m.MaSP == id && m.TrangThai == 1);

            if (sanPham == null) return NotFound();

            if (sanPham.DanhGias.Any())
            {
                ViewBag.DiemTrungBinh = Math.Round(sanPham.DanhGias.Average(d => d.DiemSao), 1);
                ViewBag.SoLuotDanhGia = sanPham.DanhGias.Count();
            }
            else
            {
                ViewBag.DiemTrungBinh = 0;
                ViewBag.SoLuotDanhGia = 0;
            }

            ViewBag.SanPhamCungLoai = await _context.SanPhams
                .Where(s => s.MaDM == sanPham.MaDM && s.MaSP != sanPham.MaSP && s.TrangThai == 1)
                .OrderByDescending(s => s.NgayTao)
                .Take(4)
                .ToListAsync();

            return View(sanPham);
        }

        // ==========================================
        // 3. THÊM ĐÁNH GIÁ SẢN PHẨM CÓ UPLOAD ẢNH
        // ==========================================
        [HttpPost]
        [Authorize]
        // 💡 Thêm tham số IFormFile HinhAnhUpload
        public async Task<IActionResult> ThemDanhGia(int MaSP, int DiemSao, string NoiDung, IFormFile HinhAnhUpload)
        {
            var maTK = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // 1. Kiểm tra khách hàng đã mua sản phẩm chưa
            var donHangHopLe = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.BienTheSanPham)
                .Where(d => d.MaTK == maTK && d.TrangThaiDH == 3 &&
                            d.ChiTietDonHangs.Any(c => c.BienTheSanPham.MaSP == MaSP))
                .FirstOrDefaultAsync();

            if (donHangHopLe == null)
            {
                TempData["ErrorMessage"] = "Bạn phải mua và nhận hàng thành công mới được phép đánh giá sản phẩm này!";
                return RedirectToAction("Details", new { id = MaSP });
            }

            // 2. Xử lý Upload file ảnh (Nếu khách có chọn ảnh)
            string duongDanAnh = ""; // Mặc định không có ảnh

            if (HinhAnhUpload != null && HinhAnhUpload.Length > 0)
            {
                // Chỉ cho phép định dạng ảnh
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(HinhAnhUpload.FileName).ToLower();

                if (allowedExtensions.Contains(extension))
                {
                    // Tạo thư mục nếu chưa có: wwwroot/uploads/reviews
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "reviews");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Đổi tên file để tránh trùng lặp
                    string uniqueFileName = Guid.NewGuid().ToString() + extension;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Copy file vào server
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await HinhAnhUpload.CopyToAsync(fileStream);
                    }

                    // Lưu đường dẫn ảo vào Database
                    duongDanAnh = "/uploads/reviews/" + uniqueFileName;
                }
            }

            // 3. Lưu Đánh giá vào Database
            var danhGiaMoi = new DanhGia
            {
                MaTK = maTK,
                MaSP = MaSP,
                MaDH = donHangHopLe.MaDH,
                DiemSao = DiemSao,
                NoiDung = NoiDung ?? "Không có nội dung",
                HinhAnh = duongDanAnh, // <--- Cập nhật đường dẫn ảnh vừa upload
                NgayDanhGia = DateTime.Now,
                TrangThai = 1
            };

            _context.DanhGias.Add(danhGiaMoi);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cảm ơn bạn đã đánh giá sản phẩm!";
            return RedirectToAction("Details", new { id = MaSP });
        }
    }
}