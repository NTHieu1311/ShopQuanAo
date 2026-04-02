using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopQuanAo.Data;
using ShopQuanAo.Models;
using System.Security.Claims;

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
        // 2. TẠO TÀI KHOẢN MỚI (Ví dụ: Tạo TK Admin)
        // ==========================================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaiKhoan taiKhoan)
        {
            // Bỏ qua kiểm tra các bảng liên kết để tránh lỗi ModelState false
            ModelState.Remove("KhachHang");
            ModelState.Remove("DonHangs");
            ModelState.Remove("DanhGias");

            if (ModelState.IsValid)
            {
                // Tự động gán ngày tạo là thời điểm hiện tại
                taiKhoan.NgayTao = DateTime.Now;

                _context.Add(taiKhoan);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo tài khoản mới thành công!";
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

            if (ModelState.IsValid)
            {
                try
                {
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
                    // Nếu tài khoản này đã mua hàng hoặc đánh giá, DB sẽ không cho xóa
                    TempData["ErrorMessage"] = "Không thể xóa! Tài khoản này đang chứa dữ liệu Khách hàng hoặc Đơn hàng. Vui lòng sử dụng chức năng Khóa tài khoản thay vì xóa.";
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
        // 1. HIỂN THỊ TRANG HỒ SƠ ADMIN
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> HoSo()
        {
            // Lấy ID tài khoản đang đăng nhập
            var maTKStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(maTKStr)) return RedirectToAction("Login", "TaiKhoan", new { area = "" });

            int maTK = int.Parse(maTKStr);
            var taiKhoan = await _context.TaiKhoans.FindAsync(maTK);

            if (taiKhoan == null) return NotFound();

            return View(taiKhoan);
        }

        // ==========================================
        // 2. CẬP NHẬT EMAIL/THÔNG TIN
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatHoSo(int MaTK, string Email)
        {
            var taiKhoan = await _context.TaiKhoans.FindAsync(MaTK);
            if (taiKhoan != null)
            {
                taiKhoan.Email = Email;
                _context.Update(taiKhoan);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật thông tin hồ sơ thành công!";
            }
            return RedirectToAction(nameof(HoSo));
        }

        // ==========================================
        // 3. ĐỔI MẬT KHẨU ADMIN
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiMatKhau(int MaTK, string MatKhauHienTai, string MatKhauMoi, string XacNhanMatKhau)
        {
            var taiKhoan = await _context.TaiKhoans.FindAsync(MaTK);
            if (taiKhoan == null) return NotFound();

            // Kiểm tra mật khẩu cũ
            if (taiKhoan.MatKhau != MatKhauHienTai)
            {
                TempData["Error"] = "Mật khẩu hiện tại không đúng!";
                return RedirectToAction(nameof(HoSo));
            }

            // Kiểm tra xác nhận mật khẩu
            if (MatKhauMoi != XacNhanMatKhau)
            {
                TempData["Error"] = "Mật khẩu mới và xác nhận không khớp!";
                return RedirectToAction(nameof(HoSo));
            }

            // Lưu mật khẩu mới
            taiKhoan.MatKhau = MatKhauMoi;
            _context.Update(taiKhoan);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đổi mật khẩu thành công! Vui lòng ghi nhớ mật khẩu mới.";
            return RedirectToAction(nameof(HoSo));
        }
    }
}