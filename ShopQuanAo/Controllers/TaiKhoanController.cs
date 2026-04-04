using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopQuanAo.Data;
using ShopQuanAo.Models;
using System.Security.Claims;
using ShopQuanAo.Helpers;

namespace ShopQuanAo.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly ShopQuanAoContext _context;

        public TaiKhoanController(ShopQuanAoContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. ĐĂNG NHẬP
        // ==========================================
        public IActionResult Login()
        {
            // Nếu đã đăng nhập rồi thì chuyển hướng luôn
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string TenDangNhap, string MatKhau)
        {
            // Mã hóa mật khẩu người dùng nhập vào để so sánh
            string matKhauDaMaHoa = MaHoaHelper.ToSHA256(MatKhau);

            // Kiểm tra trong DB (Quy ước: TrangThai = 1 là đang hoạt động)
            var taiKhoan = await _context.TaiKhoans
                .SingleOrDefaultAsync(t => t.TenDangNhap == TenDangNhap && t.MatKhau == matKhauDaMaHoa && t.TrangThai == 1);

            if (taiKhoan != null)
            {
                // Lưu thông tin người dùng vào Cookie (Claims)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, taiKhoan.MaTK.ToString()),
                    new Claim(ClaimTypes.Name, taiKhoan.TenDangNhap),
                    // Lưu Quyền: 2 là Admin, 1 là Khách
                    new Claim(ClaimTypes.Role, taiKhoan.QuyenTruyCap == 2 ? "Admin" : "Customer")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true }; // Nhớ mật khẩu

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity), authProperties);

                // KIỂM TRA QUYỀN VÀ CHUYỂN HƯỚNG
                if (taiKhoan.QuyenTruyCap == 2)
                {
                    return RedirectToAction("Index", "Home", new { area = "Admin" }); // Vào Admin Dashboard
                }
                else
                {
                    return RedirectToAction("Index", "Home"); // Vào Trang chủ mua hàng
                }
            }

            ViewBag.ErrorMessage = "Sai tên đăng nhập, mật khẩu hoặc tài khoản đã bị khóa!";
            return View();
        }

        // ==========================================
        // 2. ĐĂNG KÝ (Dành cho Khách hàng)
        // ==========================================
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string TenDangNhap, string MatKhau, string Email, string HoTen, string SoDienThoai, string DiaChi)
        {
            // [ĐÃ SỬA]: Kiểm tra trùng Tên đăng nhập
            if (await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == TenDangNhap))
            {
                ViewBag.ErrorMessage = "Tên đăng nhập này đã tồn tại, vui lòng chọn tên khác!";
                return View();
            }

            // [ĐÃ SỬA]: Kiểm tra trùng Email
            if (await _context.TaiKhoans.AnyAsync(t => t.Email == Email))
            {
                ViewBag.ErrorMessage = "Email này đã được sử dụng cho tài khoản khác!";
                return View();
            }

            // Mã hóa mật khẩu trước khi tạo tài khoản
            string matKhauDaMaHoa = MaHoaHelper.ToSHA256(MatKhau);

            // 1. Tạo Tài Khoản (QuyenTruyCap = 1 là Khách Hàng)
            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = TenDangNhap,
                MatKhau = matKhauDaMaHoa, // Lưu mật khẩu mã hóa
                Email = Email,
                QuyenTruyCap = 1,
                TrangThai = 1,
                NgayTao = DateTime.Now
            };
            _context.TaiKhoans.Add(taiKhoan);
            await _context.SaveChangesAsync(); // Lưu để sinh ra MaTK

            // 2. Tạo Hồ sơ Khách Hàng liên kết với Tài Khoản
            var khachHang = new KhachHang
            {
                MaTK = taiKhoan.MaTK,
                HoTen = HoTen,
                SoDienThoai = SoDienThoai,
                DiaChi = DiaChi,
                DiemTichLuy = 0,
                NgaySinh = DateTime.Now // Tạm thời gán mặc định
            };
            _context.KhachHangs.Add(khachHang);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // ==========================================
        // 3. ĐĂNG XUẤT
        // ==========================================
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // 4. XEM LỊCH SỬ ĐƠN HÀNG
        // ==========================================
        [Authorize]
        public async Task<IActionResult> DonHang()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login");

            var maTK = int.Parse(userIdClaim);

            // Lấy toàn bộ đơn hàng của Khách hàng này, bao gồm cả Chi tiết và Hình ảnh sản phẩm
            var donHangs = await _context.DonHangs
                .Where(d => d.MaTK == maTK)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.BienTheSanPham)
                        .ThenInclude(bt => bt.SanPham)
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            return View(donHangs);
        }

        // ==========================================
        // 5. XEM VÀ CẬP NHẬT HỒ SƠ
        // ==========================================
        [Authorize]
        public async Task<IActionResult> HoSo()
        {
            // [KHU VỰC VÁ LỖI]: Kiểm tra xem người đang bấm có phải là Admin không?
            if (User.IsInRole("Admin"))
            {
                // Nếu đúng là Admin -> Chuyển hướng thẳng vào trang Hồ sơ của Admin
                return RedirectToAction("HoSo", "TaiKhoan", new { area = "Admin" });
            }

            // Nếu là Khách hàng bình thường thì chạy logic cũ
            var maTKStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(maTKStr)) return RedirectToAction("Login");

            var maTK = int.Parse(maTKStr);
            var khachHang = await _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .FirstOrDefaultAsync(k => k.MaTK == maTK);

            if (khachHang == null) return NotFound();

            return View(khachHang);
        }

        // ==========================================
        // 6. HỦY ĐƠN HÀNG (Khách hàng tự hủy)
        // ==========================================
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> HuyDonHang(int id)
        {
            var maTK = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            // Lấy thông tin Đơn hàng và các Chi tiết đơn hàng bên trong
            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefaultAsync(d => d.MaDH == id && d.MaTK == maTK);

            if (donHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
                return RedirectToAction(nameof(DonHang));
            }

            // Chỉ cho phép hủy khi đang "Chờ xác nhận" (TrangThaiDH == 1)
            if (donHang.TrangThaiDH != 1)
            {
                TempData["ErrorMessage"] = "Chỉ có thể hủy đơn hàng đang chờ xác nhận. Nếu bạn muốn hủy đơn đang giao, vui lòng liên hệ Hotline.";
                return RedirectToAction(nameof(DonHang));
            }

            // Dùng Transaction để đảm bảo tính toàn vẹn dữ liệu
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Cập nhật trạng thái thành Đã Hủy (Trạng thái = 4)
                donHang.TrangThaiDH = 4;
                _context.Update(donHang);

                // 2. Hoàn lại số lượng tồn kho cho các sản phẩm trong đơn này
                foreach (var ct in donHang.ChiTietDonHangs)
                {
                    var bienThe = await _context.BienTheSanPhams.FindAsync(ct.MaBienThe);
                    if (bienThe != null)
                    {
                        bienThe.SoLuongTon += ct.SoLuong; // Cộng trả lại kho
                        _context.Update(bienThe);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = $"Đã hủy đơn hàng #{id} thành công!";
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Có lỗi hệ thống xảy ra khi hủy đơn hàng.";
            }

            return RedirectToAction(nameof(DonHang));
        }

        // ==========================================
        // 7. ĐỔI MẬT KHẨU
        // ==========================================
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DoiMatKhau(string MatKhauHienTai, string MatKhauMoi, string XacNhanMatKhau)
        {
            var maTK = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var taiKhoan = await _context.TaiKhoans.FindAsync(maTK);

            if (taiKhoan == null) return NotFound();

            // Mã hóa mật khẩu hiện tại để so sánh với DB
            string matKhauHienTaiDaMaHoa = MaHoaHelper.ToSHA256(MatKhauHienTai);

            // 1. Kiểm tra mật khẩu hiện tại có đúng không
            if (taiKhoan.MatKhau != matKhauHienTaiDaMaHoa)
            {
                TempData["ErrorMessage"] = "Mật khẩu hiện tại không đúng!";
                return RedirectToAction(nameof(HoSo));
            }

            // 2. Kiểm tra mật khẩu mới và xác nhận có khớp không
            if (MatKhauMoi != XacNhanMatKhau)
            {
                TempData["ErrorMessage"] = "Mật khẩu mới và xác nhận mật khẩu không khớp!";
                return RedirectToAction(nameof(HoSo));
            }

            // 3. Cập nhật mật khẩu mới vào Database (Nhớ mã hóa)
            taiKhoan.MatKhau = MaHoaHelper.ToSHA256(MatKhauMoi);
            _context.Update(taiKhoan);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction(nameof(HoSo));
        }
    }
}