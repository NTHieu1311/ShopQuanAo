using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopQuanAo.Models
{
    [Table("GiaoDichThanhToans")]
    public class GiaoDichThanhToan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaGiaoDich { get; set; }

        [ForeignKey("DonHang")]
        public int MaDH { get; set; }

        [ForeignKey("PhuongThucThanhToan")]
        public int MaPT { get; set; }

        public string MaGiaoDichDoiTac { get; set; } = null!;
        public decimal SoTien { get; set; }
        public DateTime ThoiGianThanhToan { get; set; }
        public int TrangThaiGiaoDich { get; set; }
        public string NoiDungChuyenKhoan { get; set; } = null!;

        public virtual DonHang DonHang { get; set; } = null!;
        public virtual PhuongThucThanhToan PhuongThucThanhToan { get; set; } = null!;
    }
}