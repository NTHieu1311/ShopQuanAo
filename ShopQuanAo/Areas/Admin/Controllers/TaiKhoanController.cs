using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopQuanAo.Data;
using ShopQuanAo.Models;
using System.Security.Claims;
using ShopQuanAo.Helpers;

namespace ShopQuanAo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TaiKhoanController : Controller
    {
        private readonly ShopQuanAoContext _context;

        public TaiKhoanController(ShopQuanAoContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. HIỂN THỊ DANH SÁCH TÀI KHOẢN
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var taiKhoans = await _context.TaiKhoans.OrderByDescending(t => t.NgayTao).ToListAsync();
            return View(taiKhoans);
        }

        // ==========================================
        // 2. TẠO TÀI KHOẢN MỚI
        // ==========================================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // [ĐÃ SỬA LỖI]: Thêm dấu ? vào các biến string để báo rằng chúng không bắt buộc (có thể null)
        public async Task<IActionResult> Create(TaiKhoan taiKhoan, string? HoTen, string? SoDienThoai, string? DiaChi, string? ChucVu)
        {
            ModelState.Remove("KhachHang");
            ModelState.Remove("DonHangs");
            ModelState.Remove("DanhGias");
            ModelState.Remove("QuanTriVien");

            // KIỂM TRA TRÙNG LẶP
            if (_context.TaiKhoans.Any(t => t.TenDangNhap == taiKhoan.TenDangNhap))
            {
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập này đã tồn tại!");
                return View(taiKhoan);
            }
            if (_context.TaiKhoans.Any(t => t.Email == taiKhoan.Email))
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng!");
                return View(taiKhoan);
            }

            if (ModelState.IsValid)
            {
                taiKhoan.MatKhau = MaHoaHelper.ToSHA256(taiKhoan.MatKhau);
                taiKhoan.NgayTao = DateTime.Now;

                // TỰ ĐỘNG SINH HỒ SƠ VỚI DỮ LIỆU THẬT TỪ FORM
                if (taiKhoan.QuyenTruyCap == 1)
                {
                    taiKhoan.KhachHang = new KhachHang
                    {
                        HoTen = string.IsNullOrEmpty(HoTen) ? "Khách Hàng" : HoTen,
                        SoDienThoai = string.IsNullOrEmpty(SoDienThoai) ? "" : SoDienThoai,
                        DiaChi = string.IsNullOrEmpty(DiaChi) ? "" : DiaChi,
                        NgaySinh = DateTime.Now
                    };
                }
                else if (taiKhoan.QuyenTruyCap == 2)
                {
                    taiKhoan.QuanTriVien = new QuanTriVien
                    {
                        HoTen = string.IsNullOrEmpty(HoTen) ? "Quản Trị Viên" : HoTen,
                        SoDienThoai = string.IsNullOrEmpty(SoDienThoai) ? "" : SoDienThoai,
                        ChucVu = string.IsNullOrEmpty(ChucVu) ? "Nhân Viên" : ChucVu,
                        NgayVaoLam = DateTime.Now
                    };
                }

                _context.Add(taiKhoan);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Tạo tài khoản và hồ sơ thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(taiKhoan);
        }

        // ==========================================
        // 3. SỬA / KHÓA TÀI KHOẢN
        // ==========================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var taiKhoan = await _context.TaiKhoans.FindAsync(id);
            if (taiKhoan == null) return NotFound();
            return View(taiKhoan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaiKhoan taiKhoan)
        {
            if (id != taiKhoan.MaTK) return NotFound();

            ModelState.Remove("KhachHang");
            ModelState.Remove("DonHangs");
            ModelState.Remove("DanhGias");
            ModelState.Remove("QuanTriVien");

            // Kiểm tra trùng Email khi SỬA
            if (_context.TaiKhoans.Any(t => t.Email == taiKhoan.Email && t.MaTK != id))
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng bởi một tài khoản khác!");
                return View(taiKhoan);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy thông tin tài khoản cũ từ Database lên để đối chiếu mật khẩu
                    var taiKhoanCu = await _context.TaiKhoans.AsNoTracking().FirstOrDefaultAsync(t => t.MaTK == id);

                    if (string.IsNullOrEmpty(taiKhoan.MatKhau))
                    {
                        taiKhoan.MatKhau = taiKhoanCu.MatKhau;
                    }
                    else if (taiKhoan.MatKhau != taiKhoanCu.MatKhau)
                    {
                        taiKhoan.MatKhau = MaHoaHelper.ToSHA256(taiKhoan.MatKhau);
                    }

                    _context.Update(taiKhoan);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật tài khoản thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaiKhoanExists(taiKhoan.MaTK)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(taiKhoan);
        }

        // ==========================================
        // 4. XÓA TÀI KHOẢN
        // ==========================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var taiKhoan = await _context.TaiKhoans.FirstOrDefaultAsync(m => m.MaTK == id);
            if (taiKhoan == null) return NotFound();

            return View(taiKhoan);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taiKhoan = await _context.TaiKhoans.FindAsync(id);
            if (taiKhoan != null)
            {
                try
                {
                    _context.TaiKhoans.Remove(taiKhoan);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã xóa tài khoản vĩnh viễn!";
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] = "Không thể xóa! Tài khoản này đang chứa dữ liệu. Vui lòng sử dụng chức năng Khóa tài khoản thay vì xóa.";
                    return RedirectToAction(nameof(Delete), new { id = id });
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TaiKhoanExists(int id)
        {
            return _context.TaiKhoans.Any(e => e.MaTK == id);
        }

        // ==========================================
        // 5. HIỂN THỊ TRANG HỒ SƠ ADMIN
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> HoSo()
        {
            var maTKStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(maTKStr)) return RedirectToAction("Login", "TaiKhoan", new { area = "" });

            int maTK = int.Parse(maTKStr);
            var taiKhoan = await _context.TaiKhoans.FindAsync(maTK);

            if (taiKhoan == null) return NotFound();

            return View(taiKhoan);
        }

        // ==========================================
        // 6. CẬP NHẬT EMAIL/THÔNG TIN
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatHoSo(int MaTK, string Email)
        {
            var taiKhoan = await _context.TaiKhoans.FindAsync(MaTK);
            if (taiKhoan != null)
            {
                if (_context.TaiKhoans.Any(t => t.Email == Email && t.MaTK != MaTK))
                {
                    TempData["Error"] = "Email này đã tồn tại trong hệ thống!";
                    return RedirectToAction(nameof(HoSo));
                }

                taiKhoan.Email = Email;
                _context.Update(taiKhoan);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật thông tin hồ sơ thành công!";
            }
            return RedirectToAction(nameof(HoSo));
        }

        // ==========================================
        // 7. ĐỔI MẬT KHẨU ADMIN
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiMatKhau(int MaTK, string MatKhauHienTai, string MatKhauMoi, string XacNhanMatKhau)
        {
            var taiKhoan = await _context.TaiKhoans.FindAsync(MaTK);
            if (taiKhoan == null) return NotFound();

            string matKhauHienTaiDaMaHoa = MaHoaHelper.ToSHA256(MatKhauHienTai);

            if (taiKhoan.MatKhau != matKhauHienTaiDaMaHoa)
            {
                TempData["Error"] = "Mật khẩu hiện tại không đúng!";
                return RedirectToAction(nameof(HoSo));
            }

            if (MatKhauMoi != XacNhanMatKhau)
            {
                TempData["Error"] = "Mật khẩu mới và xác nhận không khớp!";
                return RedirectToAction(nameof(HoSo));
            }

            taiKhoan.MatKhau = MaHoaHelper.ToSHA256(MatKhauMoi);
            _context.Update(taiKhoan);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đổi mật khẩu thành công! Vui lòng ghi nhớ mật khẩu mới.";
            return RedirectToAction(nameof(HoSo));
        }
    }
}