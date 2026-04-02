using System.ComponentModel.DataAnnotations.Schema;

namespace ShopQuanAo.Models
{
    [Table("ChiTietGioHangs")]
    public class ChiTietGioHang
    {
        [ForeignKey("GioHang")]
        public int MaGH { get; set; }

        [ForeignKey("BienTheSanPham")]
        public int MaBienThe { get; set; }

        public int SoLuong { get; set; }

        public virtual GioHang GioHang { get; set; } = null!;
        public virtual BienTheSanPham BienTheSanPham { get; set; } = null!;
    }
}