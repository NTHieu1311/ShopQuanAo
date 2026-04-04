using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopQuanAo.Models
{
    [Table("DonHangs")]
    public class DonHang
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaDH { get; set; }

        [ForeignKey("TaiKhoan")]
        public int MaTK { get; set; }

        public DateTime NgayDat { get; set; }
        public string? TenNguoiNhan { get; set; } = null!;
        public string? SDTNguoiNhan { get; set; } = null!;
        public string? DiaChiGiao { get; set; } = null!;
        public decimal TongTien { get; set; }
        public int TrangThaiDH { get; set; }
        public string? GhiChu { get; set; } = null!;

        public virtual TaiKhoan TaiKhoan { get; set; } = null!;
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
        public virtual ICollection<GiaoDichThanhToan> GiaoDichThanhToans { get; set; } = new List<GiaoDichThanhToan>();
        public virtual ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
    }
}