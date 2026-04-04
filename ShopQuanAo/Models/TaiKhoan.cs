using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ShopQuanAo.Models
{
    [Table("TaiKhoans")]
    public class TaiKhoan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaTK { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string? TenDangNhap { get; set; } = null!;

        public string? MatKhau { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        public string? Email { get; set; } = null!;

        public int QuyenTruyCap { get; set; }
        public int TrangThai { get; set; }

        [ValidateNever]
        public DateTime NgayTao { get; set; }

        // [ĐÃ SỬA] Thêm ValidateNever cho TẤT CẢ các bảng liên kết
        [ValidateNever]
        public virtual KhachHang? KhachHang { get; set; }

        [ValidateNever]
        public virtual QuanTriVien? QuanTriVien { get; set; }

        [ValidateNever]
        public virtual ICollection<GioHang>? GioHangs { get; set; } = new List<GioHang>();

        [ValidateNever]
        public virtual ICollection<DonHang>? DonHangs { get; set; } = new List<DonHang>();

        [ValidateNever]
        public virtual ICollection<DanhGia>? DanhGias { get; set; } = new List<DanhGia>();
    }
}