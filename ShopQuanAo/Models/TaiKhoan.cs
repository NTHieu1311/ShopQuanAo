using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopQuanAo.Models
{
    [Table("TaiKhoans")]
    public class TaiKhoan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaTK { get; set; }

        public string TenDangNhap { get; set; } = null!;
        public string MatKhau { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int QuyenTruyCap { get; set; }
        public int TrangThai { get; set; }
        public DateTime NgayTao { get; set; }

        public virtual KhachHang? KhachHang { get; set; }
        public virtual QuanTriVien? QuanTriVien { get; set; }
        public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();
        public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
        public virtual ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
    }
}