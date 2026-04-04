using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // Cần thêm dòng này

namespace ShopQuanAo.Models
{
    [Table("SanPhams")]
    public class SanPham
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaSP { get; set; }

        [ForeignKey("DanhMuc")]
        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục hợp lệ")]
        public int MaDM { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        public string TenSP { get; set; } = null!;

        // ĐÃ THÊM DẤU ? VÀO CÁC TRƯỜNG CÓ THỂ RỖNG HOẶC ĐƯỢC TẠO NGẦM
        public string? MoTaChiTiet { get; set; }

        public decimal GiaGoc { get; set; }

        public decimal GiaBan { get; set; }

        [ValidateNever] // Bỏ qua kiểm tra lỗi thuộc tính này trên giao diện
        public string? HinhAnhChinh { get; set; }

        public int TrangThai { get; set; }

        [ValidateNever]
        public DateTime NgayTao { get; set; }

        // Bỏ qua kiểm tra các bảng liên kết
        [ValidateNever]
        public virtual DanhMuc? DanhMuc { get; set; }

        [ValidateNever]
        public virtual ICollection<BienTheSanPham>? BienTheSanPhams { get; set; }

        [ValidateNever]
        public virtual ICollection<DanhGia>? DanhGias { get; set; }
    }
}