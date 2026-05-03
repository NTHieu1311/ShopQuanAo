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
        private const string MUANGAY_KEY = "MUANGAY_TEMP";

        public ThanhToanController(ShopQuanAoContext context, IVnPayService vnPayService, IConfiguration config)
        {
            _context = context;
            _vnPayService = vnPayService;
            _config = config;
        }

        // 1. HÀM LẤY GIỎ HÀNG TỪ SESSION
        public List<CartItem> LayGioHang()
        {
            var data = HttpContext.Session.GetString(CART_KEY);
            if (data == null) return new List<CartItem>();
            return JsonSerializer.Deserialize<List<CartItem>>(data);
        }

        // --- CÁC HÀM XỬ LÝ MUA NGAY TRỰC TIẾP ---
        [HttpPost]
        public async Task<IActionResult> MuaNgayTrucTiep(int MaBienThe, int SoLuong)
        {
            var bienThe = await _context.BienTheSanPhams
                .Include(b => b.SanPham)
                .FirstOrDefaultAsync(b => b.MaBienThe == MaBienThe);

            if (bienThe == null || bienThe.SoLuongTon < SoLuong)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại hoặc không đủ số lượng.";
                return RedirectToAction("Index", "SanPham");
            }

            var itemMuaNgay = new CartItem
            {
                MaBienThe = bienThe.MaBienThe,
                MaSP = bienThe.MaSP,
                TenSP = bienThe.SanPham.TenSP,
                HinhAnh = bienThe.SanPham.HinhAnhChinh,
                KichThuoc = bienThe.KichThuoc,
                MauSac = bienThe.MauSac,
                DonGia = bienThe.SanPham.GiaBan,
                SoLuong = SoLuong
            };

            HttpContext.Session.SetString(MUANGAY_KEY, JsonSerializer.Serialize(new List<CartItem> { itemMuaNgay }));

            return RedirectToAction("IndexMuaNgay");
        }

        public async Task<IActionResult> IndexMuaNgay()
        {
            var data = HttpContext.Session.GetString(MUANGAY_KEY);
            if (data == null) return RedirectToAction("Index", "Home");

            var gioHangMuaNgay = JsonSerializer.Deserialize<List<CartItem>>(data);

            var maTKStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(maTKStr)) return RedirectToAction("Login", "TaiKhoan");
            var maTK = int.Parse(maTKStr);

            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaTK == maTK);

            ViewBag.GioHang = gioHangMuaNgay;
            ViewBag.TongTien = gioHangMuaNgay.Sum(x => x.ThanhTien);
            ViewBag.PhuongThuc = await _context.PhuongThucThanhToans.Where(p => p.TrangThai == 1).ToListAsync();
            ViewBag.IsMuaNgay = true;

            return View("Index", khachHang);
        }

        // 2. HIỂN THỊ FORM THANH TOÁN (Từ giỏ hàng)
        public async Task<IActionResult> Index()
        {
            var gioHang = LayGioHang();
            if (!gioHang.Any()) return RedirectToAction("Index", "GioHang");

            var maTKStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(maTKStr)) return RedirectToAction("Login", "TaiKhoan");
            var maTK = int.Parse(maTKStr);

            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaTK == maTK);

            ViewBag.GioHang = gioHang;
            ViewBag.TongTien = gioHang.Sum(x => x.ThanhTien);
            ViewBag.PhuongThuc = await _context.PhuongThucThanhToans.Where(p => p.TrangThai == 1).ToListAsync();
            ViewBag.IsMuaNgay = false;

            return View(khachHang);
        }

        // 3. XỬ LÝ LƯU ĐƠN HÀNG VÀ CHUYỂN HƯỚNG VNPAY
        [HttpPost]
        public async Task<IActionResult> DatHang(string TenNguoiNhan, string SDTNguoiNhan, string DiaChiGiao, string GhiChu, int MaPT, bool IsMuaNgay = false)
        {
            List<CartItem> gioHang;

            if (IsMuaNgay)
            {
                var dataMuaNgay = HttpContext.Session.GetString(MUANGAY_KEY);
                if (dataMuaNgay == null) return RedirectToAction("Index", "Home");
                gioHang = JsonSerializer.Deserialize<List<CartItem>>(dataMuaNgay);
            }
            else
            {
                gioHang = LayGioHang();
            }

            if (!gioHang.Any()) return RedirectToAction("Index", "Home");

            var maTKStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(maTKStr)) return RedirectToAction("Login", "TaiKhoan");
            var maTK = int.Parse(maTKStr);

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
                    TrangThaiDH = 1, // Trạng thái: Chờ xác nhận
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

                var giaoDich = new GiaoDichThanhToan
                {
                    MaDH = donHang.MaDH,
                    MaPT = MaPT,
                    MaGiaoDichDoiTac = "",
                    SoTien = tongTien,
                    ThoiGianThanhToan = DateTime.Now,
                    TrangThaiGiaoDich = 0, // Trạng thái giao dịch ban đầu: Chưa thanh toán
                    NoiDungChuyenKhoan = $"Thanh toan don hang {donHang.MaDH}"
                };
                _context.GiaoDichThanhToans.Add(giaoDich);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                if (IsMuaNgay)
                {
                    HttpContext.Session.Remove(MUANGAY_KEY);
                }
                else
                {
                    HttpContext.Session.Remove(CART_KEY);
                }

                if (MaPT != 1) // MaPT != 1 giả sử 1 là COD, các phương thức khác là Online
                {
                    var url = _vnPayService.CreatePaymentUrl(HttpContext, donHang.MaDH, tongTien, giaoDich.NoiDungChuyenKhoan);
                    return Redirect(url);
                }

                return RedirectToAction("ThanhCong", new { id = donHang.MaDH });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Content("Lỗi đặt hàng: " + ex.Message);
            }
        }

        // 4. XỬ LÝ KẾT QUẢ TRẢ VỀ TỪ VNPAY
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

            var vnp_orderIdStr = vnpay.GetResponseData("vnp_TxnRef");
            var vnp_TransactionId = vnpay.GetResponseData("vnp_TransactionNo");
            var vnp_SecureHash = Request.Query["vnp_SecureHash"];
            var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");

            if (string.IsNullOrEmpty(vnp_orderIdStr) || !int.TryParse(vnp_orderIdStr, out int vnp_orderId))
            {
                return Content("Lỗi: Không tìm thấy mã đơn hàng từ VNPAY!");
            }

            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, _config["Vnpay:HashSecret"]);

            if (checkSignature)
            {
                if (vnp_ResponseCode == "00")
                {
                    var giaoDich = await _context.GiaoDichThanhToans.FirstOrDefaultAsync(g => g.MaDH == vnp_orderId);
                    if (giaoDich != null)
                    {
                        // Đã sửa: Chỉ ghi nhận giao dịch thành công.
                        // KHÔNG tự động cập nhật Trạng thái đơn hàng sang 2 nữa, để Admin tự bấm xác nhận!
                        giaoDich.TrangThaiGiaoDich = 1;
                        giaoDich.MaGiaoDichDoiTac = vnp_TransactionId;
                        giaoDich.ThoiGianThanhToan = DateTime.Now;
                        _context.Update(giaoDich);
                        await _context.SaveChangesAsync();
                    }
                    TempData["SuccessMessage"] = "Thanh toán VNPAY thành công! Đơn hàng đang chờ Shop xác nhận.";
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

        public IActionResult ThanhCong(int id)
        {
            ViewBag.MaDH = id;
            return View();
        }
    }
}