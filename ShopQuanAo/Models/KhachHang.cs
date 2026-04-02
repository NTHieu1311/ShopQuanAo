using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopQuanAo.Models
{
    [Table("KhachHangs")]
    public class KhachHang
    {
        [Key]
        [ForeignKey("TaiKhoan")]
        public int MaTK { get; set; }

        public string HoTen { get; set; } = null!;
        public string SoDienThoai { get; set; } = null!;
        public string DiaChi { get; set; } = null!;
        public DateTime? NgaySinh { get; set; }
        public int DiemTichLuy { get; set; }

        public virtual TaiKhoan TaiKhoan { get; set; } = null!;
    }
}