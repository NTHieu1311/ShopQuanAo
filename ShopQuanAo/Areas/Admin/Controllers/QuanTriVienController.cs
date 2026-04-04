using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopQuanAo.Data;
using ShopQuanAo.Models;
using ShopQuanAo.Helpers;

namespace ShopQuanAo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanTriVienController : Controller
    {
        private readonly ShopQuanAoContext _context;

        public QuanTriVienController(ShopQuanAoContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH
        public async Task<IActionResult> Index()
        {
            var quanTriViens = await _context.QuanTriViens.Include(q => q.TaiKhoan).ToListAsync();
            return View(quanTriViens);
        }

        // 2. THÊM MỚI
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuanTriVien quanTriVien, string TenDangNhap, string MatKhau, string Email)
        {
            ModelState.Remove("TaiKhoan");

            // ==========================================
            // KIỂM TRA TRÙNG LẶP TRƯỚC KHI LƯU
            // ==========================================
            bool trungTenDangNhap = _context.TaiKhoans.Any(t => t.TenDangNhap == TenDangNhap);
            if (trungTenDangNhap)
            {
                ModelState.AddModelError("", "Tên đăng nhập này đã tồn tại, vui lòng chọn tên khác!");
                return View(quanTriVien);
            }

            bool trungEmail = _context.TaiKhoans.Any(t => t.Email == Email);
            if (trungEmail)
            {
                ModelState.AddModelError("", "Email này đã được sử dụng cho tài khoản khác!");
                return View(quanTriVien);
            }
            // ==========================================

            if (ModelState.IsValid)
            {
                // 1. MÃ HÓA MẬT KHẨU TẠI ĐÂY TRƯỚC KHI LƯU VÀO DATABASE
                string matKhauDaMaHoa = MaHoaHelper.ToSHA256(MatKhau);

                // 2. Tạo tài khoản trước để lấy ID
                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = TenDangNhap,
                    MatKhau = matKhauDaMaHoa, // Gán mật khẩu đã mã hóa vào đây
                    Email = Email,
                    NgayTao = DateTime.Now,
                    TrangThai = 1,
                    QuyenTruyCap = 2 // BẮT BUỘC CÓ DÒNG NÀY: Ép quyền Admin cho tài khoản này
                };
                _context.TaiKhoans.Add(taiKhoan);
                await _context.SaveChangesAsync(); // Lưu để sinh ra MaTK

                // 3. Gán MaTK vừa sinh ra cho Quản trị viên
                quanTriVien.MaTK = taiKhoan.MaTK;

                // Nếu người dùng không chọn ngày vào làm, mặc định là hôm nay
                if (quanTriVien.NgayVaoLam == default)
                    quanTriVien.NgayVaoLam = DateTime.Now;

                _context.QuanTriViens.Add(quanTriVien);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thêm quản trị viên thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(quanTriVien);
        }

        // 3. SỬA
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Tìm theo MaTK
            var quanTriVien = await _context.QuanTriViens.Include(q => q.TaiKhoan).FirstOrDefaultAsync(q => q.MaTK == id);
            if (quanTriVien == null) return NotFound();

            return View(quanTriVien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, QuanTriVien quanTriVien)
        {
            if (id != quanTriVien.MaTK) return NotFound();
            ModelState.Remove("TaiKhoan");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(quanTriVien);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuanTriVienExists(quanTriVien.MaTK)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(quanTriVien);
        }

        // 4. XÓA
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var qtv = await _context.QuanTriViens.Include(q => q.TaiKhoan).FirstOrDefaultAsync(m => m.MaTK == id);
            if (qtv == null) return NotFound();
            return View(qtv);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var qtv = await _context.QuanTriViens.Include(q => q.TaiKhoan).FirstOrDefaultAsync(q => q.MaTK == id);
            if (qtv != null)
            {
                var tk = qtv.TaiKhoan;
                _context.QuanTriViens.Remove(qtv);
                if (tk != null) _context.TaiKhoans.Remove(tk); // Xóa luôn tài khoản liên kết
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa quản trị viên!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool QuanTriVienExists(int id)
        {
            return _context.QuanTriViens.Any(e => e.MaTK == id);
        }
    }
}