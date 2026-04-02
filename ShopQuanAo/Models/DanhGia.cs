using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopQuanAo.Models
{
    [Table("DanhGias")]
    public class DanhGia
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaDanhGia { get; set; }

        [ForeignKey("TaiKhoan")]
        public int MaTK { get; set; }

        [ForeignKey("SanPham")]
        public int MaSP { get; set; }

        [ForeignKey("DonHang")]
        public int MaDH { get; set; }

        public int DiemSao { get; set; }
        public string NoiDung { get; set; } = null!;
        public string HinhAnh { get; set; } = null!;
        public DateTime NgayDanhGia { get; set; }
        public int TrangThai { get; set; }

        public virtual TaiKhoan TaiKhoan { get; set; } = null!;
        public virtual SanPham SanPham { get; set; } = null!;
        public virtual DonHang DonHang { get; set; } = null!;
    }
}