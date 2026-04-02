using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopQuanAo.Data;
using ShopQuanAo.Models;
using ShopQuanAo.Services;
using ShopQuanAo.Libraries;
using System.Security.Claims;
using System.Text.Json;

namespace ShopQuanAo.Controllers
{
    [Authorize]
    public class ThanhToanController : Controller
    {
        private readonly ShopQuanAoContext _context;
        private readonly IVnPayService _vnPayService;
        private readonly IConfiguration _config;
        private const string CART_KEY = "MYCART";

        public ThanhToanController(ShopQuanAoContext context, IVnPayService vnPayService, IConfiguration config)
        {
            _context = context;
            _vnPayService = vnPayService;
            _config = config;
        }

        // ==========================================
        // 1. HÀM LẤY GIỎ HÀNG TỪ SESSION
        // ==========================================
        public List<CartItem> LayGioHang()
        {
            var data = HttpContext.Session.GetString(CART_KEY);
            if (data == null) return new List<CartItem>();
            return JsonSerializer.Deserialize<List<CartItem>>(data);
        }

        // ==========================================
        // 2. HIỂN THỊ FORM THANH TOÁN
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var gioHang = LayGioHang();
            if (!gioHang.Any()) return RedirectToAction("Index", "GioHang");

            var maTK = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaTK == maTK);

            ViewBag.GioHang = gioHang;
            ViewBag.TongTien = gioHang.Sum(x => x.ThanhTien);
            ViewBag.PhuongThuc = await _context.PhuongThucThanhToans.Where(p => p.TrangThai == 1).ToListAsync();

            return View(khachHang);
        }

        // ==========================================
        // 3. XỬ LÝ LƯU ĐƠN HÀNG VÀ CHUYỂN HƯỚNG VNPAY
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> DatHang(string TenNguoiNhan, string SDTNguoiNhan, string DiaChiGiao, string GhiChu, int MaPT)
        {
            var gioHang = LayGioHang();
            if (!gioHang.Any()) return RedirectToAction("Index", "Home");

            var maTK = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var tongTien = gioHang.Sum(x => x.ThanhTien);
                var donHang = new DonHang
                {
                    MaTK = maTK,
                    NgayDat = DateTime.Now,
                    TenNguoiNhan = TenNguoiNhan,
                    SDTNguoiNhan = SDTNguoiNhan,
                    DiaChiGiao = DiaChiGiao,
                    TongTien = tongTien,
                    TrangThaiDH = 1, // 1: Chờ xác nhận
                    GhiChu = string.IsNullOrEmpty(GhiChu) ? "Không có ghi chú" : GhiChu
                };
                _context.DonHangs.Add(donHang);
                await _context.SaveChangesAsync();

                foreach (var item in gioHang)
                {
                    var chiTiet = new ChiTietDonHang { MaDH = donHang.MaDH, MaBienThe = item.MaBienThe, SoLuong = item.SoLuong, DonGiaXuat = item.DonGia };
                    _context.ChiTietDonHangs.Add(chiTiet);

                    var bienThe = await _context.BienTheSanPhams.FindAsync(item.MaBienThe);
                    if (bienThe != null)
                    {
                        bienThe.SoLuongTon -= item.SoLuong;
                        _context.Update(bienThe);
                    }
                }

                // Ghi nhận giao dịch đang chờ
                var giaoDich = new GiaoDichThanhToan
                {
                    MaDH = donHang.MaDH,
                    MaPT = MaPT,
                    MaGiaoDichDoiTac = "",
                    SoTien = tongTien,
                    ThoiGianThanhToan = DateTime.Now,
                    TrangThaiGiaoDich = 0,
                    NoiDungChuyenKhoan = $"Thanh toan don hang {donHang.MaDH}"
                };
                _context.GiaoDichThanhToans.Add(giaoDich);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                HttpContext.Session.Remove(CART_KEY); // Đặt xong thì xóa giỏ

                // 💡 ĐÃ SỬA ĐIỀU KIỆN Ở ĐÂY: Nếu không phải Tiền mặt COD (MaPT = 1) thì đều đẩy qua VNPAY
                if (MaPT != 1)
                {
                    // Chuyển hướng khách hàng sang trang web của VNPAY
                    var url = _vnPayService.CreatePaymentUrl(HttpContext, donHang.MaDH, tongTien, giaoDich.NoiDungChuyenKhoan);
                    return Redirect(url);
                }

                // Nếu là COD (Tiền mặt), thì về trang Cảm ơn luôn
                return RedirectToAction("ThanhCong", new { id = donHang.MaDH });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Content("Lỗi đặt hàng: " + ex.Message);
            }
        }

        // ==========================================
        // 4. HỨNG KẾT QUẢ TỪ VNPAY TRẢ VỀ (CALLBACK)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> PaymentCallback()
        {
            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in Request.Query)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }

            var vnp_orderId = Convert.ToInt32(vnpay.GetResponseData("vnp_TxnRef"));
            var vnp_TransactionId = vnpay.GetResponseData("vnp_TransactionNo");
            var vnp_SecureHash = Request.Query["vnp_SecureHash"];
            var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");

            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, _config["Vnpay:HashSecret"]);

            if (checkSignature)
            {
                if (vnp_ResponseCode == "00") // 00 = Thanh toán thành công
                {
                    var giaoDich = await _context.GiaoDichThanhToans.FirstOrDefaultAsync(g => g.MaDH == vnp_orderId);
                    if (giaoDich != null)
                    {
                        giaoDich.TrangThaiGiaoDich = 1; // Đã thanh toán
                        giaoDich.MaGiaoDichDoiTac = vnp_TransactionId;
                        giaoDich.ThoiGianThanhToan = DateTime.Now;
                        _context.Update(giaoDich);
                        await _context.SaveChangesAsync();
                    }
                    TempData["SuccessMessage"] = "Thanh toán VNPAY thành công!";
                    return RedirectToAction("ThanhCong", new { id = vnp_orderId });
                }
                else
                {
                    TempData["ErrorMessage"] = $"Lỗi thanh toán VNPAY. Mã lỗi: {vnp_ResponseCode}";
                    return RedirectToAction("ThanhCong", new { id = vnp_orderId });
                }
            }

            return Content("Lỗi: Chữ ký bảo mật không hợp lệ!");
        }

        // ==========================================
        // 5. TRANG THÔNG BÁO THÀNH CÔNG
        // ==========================================
        public IActionResult ThanhCong(int id)
        {
            ViewBag.MaDH = id;
            return View();
        }
    }
}