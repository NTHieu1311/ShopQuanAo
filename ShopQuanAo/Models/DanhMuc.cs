using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopQuanAo.Models
{
    [Table("DanhMucs")]
    public class DanhMuc
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int MaDM { get; set; }

        public string? TenDM { get; set; } = null!;
        public string? MoTa { get; set; } = null!;
        public int TrangThai { get; set; }

        public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
    }
}