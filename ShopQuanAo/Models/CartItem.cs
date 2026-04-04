namespace ShopQuanAo.Models
{
    public class CartItem
    {
        public int MaBienThe { get; set; } // Khóa chính để xác định chính xác Màu/Size
        public int MaSP { get; set; }
        public string? TenSP { get; set; } = null!;
        public string? HinhAnh { get; set; } = null!;
        public string? MauSac { get; set; } = null!;
        public string? KichThuoc { get; set; } = null!;
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }

        // Tự động tính Thành tiền
        public decimal ThanhTien => DonGia * SoLuong;
    }
}