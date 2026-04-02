using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopQuanAo.Models
{
    [Table("GioHangs")]
    public class GioHang
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaGH { get; set; }

        [ForeignKey("TaiKhoan")]
        public int MaTK { get; set; }

        public DateTime NgayTao { get; set; }

        public virtual TaiKhoan TaiKhoan { get; set; } = null!;
        public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();
    }
}