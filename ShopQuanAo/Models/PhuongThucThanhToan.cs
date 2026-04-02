using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopQuanAo.Models
{
    [Table("PhuongThucThanhToans")]
    public class PhuongThucThanhToan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaPT { get; set; }

        public string TenPhuongThuc { get; set; } = null!;
        public string MoTa { get; set; } = null!;
        public int TrangThai { get; set; }

        public virtual ICollection<GiaoDichThanhToan> GiaoDichThanhToans { get; set; } = new List<GiaoDichThanhToan>();
    }
}