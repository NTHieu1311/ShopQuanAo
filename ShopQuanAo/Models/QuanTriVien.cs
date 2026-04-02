using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopQuanAo.Models
{
    [Table("QuanTriViens")]
    public class QuanTriVien
    {
        [Key]
        [ForeignKey("TaiKhoan")]
        public int MaTK { get; set; }

        public string HoTen { get; set; } = null!;
        public string SoDienThoai { get; set; } = null!;
        public string ChucVu { get; set; } = null!;
        public DateTime NgayVaoLam { get; set; }

        public virtual TaiKhoan TaiKhoan { get; set; } = null!;
    }
}