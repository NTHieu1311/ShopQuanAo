using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// BẮT BUỘC THÊM DÒNG NÀY ĐỂ BỎ QUA KIỂM TRA LỖI
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ShopQuanAo.Models
{
    [Table("QuanTriViens")]
    public class QuanTriVien
    {
        [Key]
        [ForeignKey("TaiKhoan")]
        public int MaTK { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string? HoTen { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string? SoDienThoai { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập chức vụ")]
        public string? ChucVu { get; set; } = null!;

        [ValidateNever]
        public DateTime NgayVaoLam { get; set; }

        // [ĐÃ SỬA] Thêm ValidateNever và dấu ? để trình duyệt bỏ qua không kiểm tra
        [ValidateNever]
        public virtual TaiKhoan? TaiKhoan { get; set; }
    }
}