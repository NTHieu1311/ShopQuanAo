using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using ShopQuanAo.Models;
using System.Linq;

namespace ShopQuanAo.Data
{
    public class ShopQuanAoContext : DbContext
    {
        public ShopQuanAoContext(DbContextOptions<ShopQuanAoContext> options) : base(options) { }

        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<QuanTriVien> QuanTriViens { get; set; }
        public DbSet<DanhMuc> DanhMucs { get; set; }
        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<BienTheSanPham> BienTheSanPhams { get; set; }
        public DbSet<GioHang> GioHangs { get; set; }
        public DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public DbSet<PhuongThucThanhToan> PhuongThucThanhToans { get; set; }
        public DbSet<GiaoDichThanhToan> GiaoDichThanhToans { get; set; }
        public DbSet<DanhGia> DanhGias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Cấu hình Khóa chính kép (Composite Keys) cho Chi Tiết
            modelBuilder.Entity<ChiTietDonHang>()
                .HasKey(c => new { c.MaDH, c.MaBienThe });

            modelBuilder.Entity<ChiTietGioHang>()
                .HasKey(c => new { c.MaGH, c.MaBienThe });

            // 2. Cấu hình quan hệ 1-1
            modelBuilder.Entity<KhachHang>()
                .HasOne(kh => kh.TaiKhoan)
                .WithOne(tk => tk.KhachHang)
                .HasForeignKey<KhachHang>(kh => kh.MaTK);

            modelBuilder.Entity<QuanTriVien>()
                .HasOne(qtv => qtv.TaiKhoan)
                .WithOne(tk => tk.QuanTriVien)
                .HasForeignKey<QuanTriVien>(qtv => qtv.MaTK);

            
            // 💡 THÊM ĐOẠN NÀY ĐỂ FIX CẢNH BÁO DECIMAL
            
            modelBuilder.Entity<ChiTietDonHang>()
                .Property(c => c.DonGiaXuat)
                .HasColumnType("decimal(18, 0)");

            modelBuilder.Entity<DonHang>()
                .Property(d => d.TongTien)
                .HasColumnType("decimal(18, 0)");

            modelBuilder.Entity<GiaoDichThanhToan>()
                .Property(g => g.SoTien)
                .HasColumnType("decimal(18, 0)");

            modelBuilder.Entity<SanPham>()
                .Property(s => s.GiaBan)
                .HasColumnType("decimal(18, 0)");

            modelBuilder.Entity<SanPham>()
                .Property(s => s.GiaGoc)
                .HasColumnType("decimal(18, 0)");

            modelBuilder.Entity<DanhGia>()
                .HasOne(d => d.TaiKhoan)
                .WithMany(t => t.DanhGias)
                .HasForeignKey(d => d.MaTK)
                .OnDelete(DeleteBehavior.NoAction); // Không xóa dây chuyền

            modelBuilder.Entity<DanhGia>()
                .HasOne(d => d.SanPham)
                .WithMany(s => s.DanhGias)
                .HasForeignKey(d => d.MaSP)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<DanhGia>()
                .HasOne(d => d.DonHang)
                .WithMany(dh => dh.DanhGias)
                .HasForeignKey(d => d.MaDH)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}