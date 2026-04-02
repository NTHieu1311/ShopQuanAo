using System.ComponentModel.DataAnnotations.Schema;

namespace ShopQuanAo.Models
{
    [Table("ChiTietDonHangs")]
    public class ChiTietDonHang
    {
        [ForeignKey("DonHang")]
        public int MaDH { get; set; }

        [ForeignKey("BienTheSanPham")]
        public int MaBienThe { get; set; }

        public int SoLuong { get; set; }
        public decimal DonGiaXuat { get; set; }

        public virtual DonHang DonHang { get; set; } = null!;
        public virtual BienTheSanPham BienTheSanPham { get; set; } = null!;
    }
}