using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopQuanAo.Data;
using ShopQuanAo.Models;
using System.Text.Json;

namespace ShopQuanAo.Controllers
{
    public class GioHangController : Controller
    {
        private readonly ShopQuanAoContext _context;
        private const string CART_KEY = "MYCART"; // Tên chìa khóa lưu Session

        public GioHangController(ShopQuanAoContext context)
        {
            _context = context;
        }

        // HÀM HỖ TRỢ: Lấy giỏ hàng từ Session ra
        public List<CartItem> LayGioHang()
        {
            var data = HttpContext.Session.GetString(CART_KEY);
            if (data == null)
            {
                return new List<CartItem>(); // Nếu chưa có thì trả về giỏ rỗng
            }
            // Giải mã chuỗi JSON thành Danh sách CartItem
            return JsonSerializer.Deserialize<List<CartItem>>(data);
        }

        // ==========================================
        // 1. THÊM SẢN PHẨM VÀO GIỎ
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Them(int MaBienThe, int SoLuong)
        {
            var gioHang = LayGioHang();
            // Kiểm tra xem món này (đúng màu, đúng size) đã có trong giỏ chưa
            var item = gioHang.SingleOrDefault(p => p.MaBienThe == MaBienThe);

            if (item != null)
            {
                // Nếu có rồi thì cộng dồn số lượng
                item.SoLuong += SoLuong;
            }
            else
            {
                // Nếu chưa có thì chui vào Database lấy thông tin sản phẩm ra
                var bienThe = await _context.BienTheSanPhams
                    .Include(b => b.SanPham)
                    .SingleOrDefaultAsync(b => b.MaBienThe == MaBienThe);

                if (bienThe != null)
                {
                    item = new CartItem
                    {
                        MaBienThe = bienThe.MaBienThe,
                        MaSP = bienThe.SanPham.MaSP,
                        TenSP = bienThe.SanPham.TenSP,
                        HinhAnh = bienThe.SanPham.HinhAnhChinh,
                        MauSac = bienThe.MauSac,
                        KichThuoc = bienThe.KichThuoc,
                        DonGia = bienThe.SanPham.GiaBan,
                        SoLuong = SoLuong
                    };
                    gioHang.Add(item); // Nhét vào giỏ
                }
            }

            // Mã hóa thành JSON và Lưu lại vào Session
            HttpContext.Session.SetString(CART_KEY, JsonSerializer.Serialize(gioHang));

            TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng!";
            return RedirectToAction("Index"); // Chuyển sang trang xem Giỏ hàng
        }

        // ==========================================
        // 2. HIỂN THỊ TRANG GIỎ HÀNG
        // ==========================================
        public IActionResult Index()
        {
            var gioHang = LayGioHang();
            return View(gioHang);
        }

        // ==========================================
        // 3. XÓA SẢN PHẨM KHỎI GIỎ
        // ==========================================
        public IActionResult Xoa(int id) // id ở đây là MaBienThe
        {
            var gioHang = LayGioHang();
            var item = gioHang.SingleOrDefault(p => p.MaBienThe == id);
            if (item != null)
            {
                gioHang.Remove(item);
                // Cập nhật lại Session sau khi xóa
                HttpContext.Session.SetString(CART_KEY, JsonSerializer.Serialize(gioHang));
            }
            return RedirectToAction("Index");
        }
        // ==========================================
        // TÍNH NĂNG MUA NGAY (Lưu giỏ hàng và nhảy sang Thanh Toán luôn)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> MuaNgay(int MaBienThe, int SoLuong)
        {
            var gioHang = LayGioHang();
            var item = gioHang.SingleOrDefault(p => p.MaBienThe == MaBienThe);

            if (item != null)
            {
                item.SoLuong += SoLuong;
            }
            else
            {
                var bienThe = await _context.BienTheSanPhams
                    .Include(b => b.SanPham)
                    .SingleOrDefaultAsync(b => b.MaBienThe == MaBienThe);

                if (bienThe != null)
                {
                    item = new CartItem
                    {
                        MaBienThe = bienThe.MaBienThe,
                        MaSP = bienThe.SanPham.MaSP,
                        TenSP = bienThe.SanPham.TenSP,
                        HinhAnh = bienThe.SanPham.HinhAnhChinh,
                        MauSac = bienThe.MauSac,
                        KichThuoc = bienThe.KichThuoc,
                        DonGia = bienThe.SanPham.GiaBan,
                        SoLuong = SoLuong
                    };
                    gioHang.Add(item);
                }
            }

            HttpContext.Session.SetString(CART_KEY, JsonSerializer.Serialize(gioHang));

            // LƯU Ý SỰ KHÁC BIỆT: Thay vì về "Index" của Giỏ hàng, nó chuyển thẳng sang "ThanhToan"
            return RedirectToAction("Index", "ThanhToan");
        }
    }
}