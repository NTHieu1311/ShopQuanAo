using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopQuanAo.Data;
using ShopQuanAo.Models;

namespace ShopQuanAo.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class KhachHangController : Controller
	{
		private readonly ShopQuanAoContext _context;

		public KhachHangController(ShopQuanAoContext context)
		{
			_context = context;
		}

		
		// 1. HIỂN THỊ DANH SÁCH & TÌM KIẾM KHÁCH HÀNG
		
		public async Task<IActionResult> Index(string searchKeyword)
		{
			// Kéo theo bảng TaiKhoan để lấy được Email và Trạng Thái đăng nhập
			var query = _context.KhachHangs
				.Include(k => k.TaiKhoan)
				.AsQueryable();

			// Chức năng tìm kiếm: Lọc theo Tên, SĐT hoặc Email
			if (!string.IsNullOrEmpty(searchKeyword))
			{
				searchKeyword = searchKeyword.Trim();

				query = query.Where(k =>
					k.HoTen.Contains(searchKeyword) ||
					k.SoDienThoai.Contains(searchKeyword) ||
					k.TaiKhoan.Email.Contains(searchKeyword)
				);
			}

			var khachHangs = await query.OrderByDescending(k => k.MaTK).ToListAsync();

			ViewBag.SearchKeyword = searchKeyword;

			return View(khachHangs);
		}

		
		// 2. XEM CHI TIẾT VÀ KHÓA/MỞ KHÓA TÀI KHOẢN (EDIT)
		
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null) return NotFound();

			// ID ở đây chính là MaTK
			var khachHang = await _context.KhachHangs
				.Include(k => k.TaiKhoan)
				.FirstOrDefaultAsync(k => k.MaTK == id);

			if (khachHang == null) return NotFound();
			return View(khachHang);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		// Tham số truyền vào: MaTK, DiemTichLuy và TrangThai (của bảng TaiKhoan)
		public async Task<IActionResult> Edit(int id, int DiemTichLuy, int TrangThai)
		{
			var khachHang = await _context.KhachHangs
				.Include(k => k.TaiKhoan)
				.FirstOrDefaultAsync(k => k.MaTK == id);

			if (khachHang == null) return NotFound();

			try
			{
				// Cập nhật điểm tích lũy của Khách
				khachHang.DiemTichLuy = DiemTichLuy;

				// Cập nhật trạng thái (Khóa/Mở) của Tài Khoản
				khachHang.TaiKhoan.TrangThai = TrangThai;

				_context.Update(khachHang);
				await _context.SaveChangesAsync();
				TempData["SuccessMessage"] = "Cập nhật thông tin khách hàng thành công!";
			}
			catch (DbUpdateException)
			{
				TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu dữ liệu!";
			}

			return RedirectToAction(nameof(Index));
		}
	}
}