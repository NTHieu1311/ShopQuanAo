using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopQuanAo.Data;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace ShopQuanAo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ShopQuanAoContext _context;

        public HomeController(ShopQuanAoContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Thống kê tổng quan (Dashboard)
            var doanhThu = await _context.DonHangs
                .Where(d => d.TrangThaiDH == 3)
                .SumAsync(d => (decimal?)d.TongTien) ?? 0;

            var donMoi = await _context.DonHangs.CountAsync(d => d.TrangThaiDH == 1);
            var soKhachHang = await _context.KhachHangs.CountAsync();
            var sapHetHang = await _context.BienTheSanPhams.CountAsync(b => b.SoLuongTon < 10);

            // 2. Dữ liệu biểu đồ Donut (Trạng thái đơn hàng)
            var thongKeDonHang = new List<int>
            {
                await _context.DonHangs.CountAsync(d => d.TrangThaiDH == 1),
                await _context.DonHangs.CountAsync(d => d.TrangThaiDH == 2),
                await _context.DonHangs.CountAsync(d => d.TrangThaiDH == 3),
                await _context.DonHangs.CountAsync(d => d.TrangThaiDH == 4)
            };

            // 3. Dữ liệu biểu đồ Line (Doanh thu 12 tháng)
            var doanhThu12Thang = new List<decimal>();
            int namHienTai = DateTime.Now.Year;

            for (int i = 1; i <= 12; i++)
            {
                var dtThang = await _context.DonHangs
                    .Where(d => d.TrangThaiDH == 3 && d.NgayDat.Month == i && d.NgayDat.Year == namHienTai)
                    .SumAsync(d => (decimal?)d.TongTien) ?? 0;
                doanhThu12Thang.Add(dtThang);
            }

            ViewBag.DoanhThu = doanhThu;
            ViewBag.DonMoi = donMoi;
            ViewBag.SoKhachHang = soKhachHang;
            ViewBag.SapHetHang = sapHetHang;
            ViewBag.ThongKeDonHang = thongKeDonHang;
            ViewBag.DoanhThuData = doanhThu12Thang;

            return View();
        }

        // ================= EXPORT EXCEL (BẢN CHUẨN EPPLUS 7) =================
        public async Task<IActionResult> ExportToExcel(string type, DateTime? fromDate, DateTime? toDate, int? month, int? year, int? quarter)
        {
            var query = _context.DonHangs
                .Include(d => d.TaiKhoan)
                    .ThenInclude(t => t.KhachHang)
                .Where(d => d.TrangThaiDH == 3)
                .AsQueryable();

            string titleDetail = "TẤT CẢ THỜI GIAN";

            // Xử lý bộ lọc và chuỗi tiêu đề hiển thị trong file Excel
            switch (type)
            {
                case "date":
                    if (fromDate.HasValue && toDate.HasValue)
                    {
                        query = query.Where(d => d.NgayDat.Date >= fromDate.Value.Date && d.NgayDat.Date <= toDate.Value.Date);
                        titleDetail = $"TỪ NGÀY {fromDate.Value:dd/MM/yyyy} ĐẾN NGÀY {toDate.Value:dd/MM/yyyy}";
                    }
                    break;
                case "month":
                    if (month.HasValue && year.HasValue)
                    {
                        query = query.Where(d => d.NgayDat.Month == month && d.NgayDat.Year == year);
                        titleDetail = $"THÁNG {month} NĂM {year}";
                    }
                    break;
                case "year":
                    if (year.HasValue)
                    {
                        query = query.Where(d => d.NgayDat.Year == year);
                        titleDetail = $"NĂM {year}";
                    }
                    break;
                case "quarter":
                    if (quarter.HasValue && year.HasValue)
                    {
                        int start = (quarter.Value - 1) * 3 + 1;
                        int end = start + 2;
                        query = query.Where(d => d.NgayDat.Year == year && d.NgayDat.Month >= start && d.NgayDat.Month <= end);
                        titleDetail = $"QUÝ {quarter} NĂM {year}";
                    }
                    break;
            }

            var data = await query.OrderByDescending(d => d.NgayDat).ToListAsync();

            // --- CẤU HÌNH BẢN QUYỀN CHO EPPLUS 7 ---
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // --------------------------------------

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("BaoCaoDoanhThu");

                // Tiêu đề chính (Dòng 1)
                ws.Cells[1, 1, 1, 5].Merge = true;
                ws.Cells[1, 1].Value = "BÁO CÁO DOANH THU HỆ THỐNG";
                ws.Cells[1, 1].Style.Font.Size = 16;
                ws.Cells[1, 1].Style.Font.Bold = true;
                ws.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Tiêu đề phụ hiển thị thời gian đã lọc (Dòng 2)
                ws.Cells[2, 1, 2, 5].Merge = true;
                ws.Cells[2, 1].Value = titleDetail;
                ws.Cells[2, 1].Style.Font.Size = 12;
                ws.Cells[2, 1].Style.Font.Italic = true;
                ws.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[2, 1].Style.Font.Color.SetColor(Color.Gray);

                // Header bảng (Dòng 4)
                int headerRow = 4;
                ws.Cells[headerRow, 1].Value = "Mã ĐH";
                ws.Cells[headerRow, 2].Value = "Ngày Đặt";
                ws.Cells[headerRow, 3].Value = "Khách Hàng";
                ws.Cells[headerRow, 4].Value = "Số Điện Thoại";
                ws.Cells[headerRow, 5].Value = "Tổng Tiền";

                using (var r = ws.Cells[headerRow, 1, headerRow, 5])
                {
                    r.Style.Font.Bold = true;
                    r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 51, 102));
                    r.Style.Font.Color.SetColor(Color.White);
                    r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    r.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Đổ dữ liệu
                int row = headerRow + 1;
                foreach (var item in data)
                {
                    ws.Cells[row, 1].Value = "#" + item.MaDH;
                    ws.Cells[row, 2].Value = item.NgayDat.ToString("dd/MM/yyyy HH:mm");
                    ws.Cells[row, 3].Value = item.TaiKhoan?.KhachHang?.HoTen ?? "Khách lẻ";
                    ws.Cells[row, 4].Value = item.TaiKhoan?.KhachHang?.SoDienThoai ?? "N/A";
                    ws.Cells[row, 5].Value = item.TongTien;
                    ws.Cells[row, 5].Style.Numberformat.Format = "#,##0 \"đ\"";

                    ws.Cells[row, 1, row, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[row, 1, row, 5].Style.Border.Bottom.Color.SetColor(Color.LightGray);
                    row++;
                }

                // Dòng tổng cộng (Footer)
                if (data.Count > 0)
                {
                    ws.Cells[row, 4].Value = "TỔNG CỘNG:";
                    ws.Cells[row, 4].Style.Font.Bold = true;
                    ws.Cells[row, 5].Formula = $"SUM(E{headerRow + 1}:E{row - 1})";
                    ws.Cells[row, 5].Style.Font.Bold = true;
                    ws.Cells[row, 5].Style.Font.Color.SetColor(Color.Red);
                    ws.Cells[row, 5].Style.Numberformat.Format = "#,##0 \"đ\"";
                }

                ws.Cells.AutoFitColumns();

                var fileName = $"BaoCao_{DateTime.Now:ddMMyyyy_HHmm}.xlsx";
                return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
    }
}