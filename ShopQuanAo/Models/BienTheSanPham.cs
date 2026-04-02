using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopQuanAo.Models
{
    [Table("BienTheSanPhams")]
    public class BienTheSanPham
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaBienThe { get; set; }

        [ForeignKey("SanPham")]
        public int MaSP { get; set; }

        public string KichThuoc { get; set; } = null!;
        public string MauSac { get; set; } = null!;
        public int SoLuongTon { get; set; }

        public virtual SanPham SanPham { get; set; } = null!;
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
        public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();
    }
}