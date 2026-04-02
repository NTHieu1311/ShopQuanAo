using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopQuanAo.Models
{
    [Table("SanPhams")]
    public class SanPham
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaSP { get; set; }

        [ForeignKey("DanhMuc")]
        public int MaDM { get; set; }

        public string TenSP { get; set; } = null!;
        public string MoTaChiTiet { get; set; } = null!;
        public decimal GiaGoc { get; set; }
        public decimal GiaBan { get; set; }
        public string HinhAnhChinh { get; set; } = null!;
        public int TrangThai { get; set; }
        public DateTime NgayTao { get; set; }

        public virtual DanhMuc DanhMuc { get; set; } = null!;
        public virtual ICollection<BienTheSanPham> BienTheSanPhams { get; set; } = new List<BienTheSanPham>();
        public virtual ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
    }
}