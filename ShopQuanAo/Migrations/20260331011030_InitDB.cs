using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopQuanAo.Migrations
{
    /// <inheritdoc />
    public partial class InitDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DanhMucs",
                columns: table => new
                {
                    MaDM = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDM = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMucs", x => x.MaDM);
                });

            migrationBuilder.CreateTable(
                name: "PhuongThucThanhToans",
                columns: table => new
                {
                    MaPT = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenPhuongThuc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhuongThucThanhToans", x => x.MaPT);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoans",
                columns: table => new
                {
                    MaTK = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuyenTruyCap = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoans", x => x.MaTK);
                });

            migrationBuilder.CreateTable(
                name: "SanPhams",
                columns: table => new
                {
                    MaSP = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDM = table.Column<int>(type: "int", nullable: false),
                    TenSP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTaChiTiet = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GiaGoc = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    GiaBan = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    HinhAnhChinh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPhams", x => x.MaSP);
                    table.ForeignKey(
                        name: "FK_SanPhams_DanhMucs_MaDM",
                        column: x => x.MaDM,
                        principalTable: "DanhMucs",
                        principalColumn: "MaDM",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DonHangs",
                columns: table => new
                {
                    MaDH = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTK = table.Column<int>(type: "int", nullable: false),
                    NgayDat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TenNguoiNhan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SDTNguoiNhan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiaChiGiao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    TrangThaiDH = table.Column<int>(type: "int", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonHangs", x => x.MaDH);
                    table.ForeignKey(
                        name: "FK_DonHangs_TaiKhoans_MaTK",
                        column: x => x.MaTK,
                        principalTable: "TaiKhoans",
                        principalColumn: "MaTK",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GioHangs",
                columns: table => new
                {
                    MaGH = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTK = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GioHangs", x => x.MaGH);
                    table.ForeignKey(
                        name: "FK_GioHangs_TaiKhoans_MaTK",
                        column: x => x.MaTK,
                        principalTable: "TaiKhoans",
                        principalColumn: "MaTK",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KhachHangs",
                columns: table => new
                {
                    MaTK = table.Column<int>(type: "int", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DiemTichLuy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHangs", x => x.MaTK);
                    table.ForeignKey(
                        name: "FK_KhachHangs_TaiKhoans_MaTK",
                        column: x => x.MaTK,
                        principalTable: "TaiKhoans",
                        principalColumn: "MaTK",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuanTriViens",
                columns: table => new
                {
                    MaTK = table.Column<int>(type: "int", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChucVu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayVaoLam = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuanTriViens", x => x.MaTK);
                    table.ForeignKey(
                        name: "FK_QuanTriViens_TaiKhoans_MaTK",
                        column: x => x.MaTK,
                        principalTable: "TaiKhoans",
                        principalColumn: "MaTK",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BienTheSanPhams",
                columns: table => new
                {
                    MaBienThe = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaSP = table.Column<int>(type: "int", nullable: false),
                    KichThuoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MauSac = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoLuongTon = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BienTheSanPhams", x => x.MaBienThe);
                    table.ForeignKey(
                        name: "FK_BienTheSanPhams_SanPhams_MaSP",
                        column: x => x.MaSP,
                        principalTable: "SanPhams",
                        principalColumn: "MaSP",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DanhGias",
                columns: table => new
                {
                    MaDanhGia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTK = table.Column<int>(type: "int", nullable: false),
                    MaSP = table.Column<int>(type: "int", nullable: false),
                    MaDH = table.Column<int>(type: "int", nullable: false),
                    DiemSao = table.Column<int>(type: "int", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayDanhGia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGias", x => x.MaDanhGia);
                    table.ForeignKey(
                        name: "FK_DanhGias_DonHangs_MaDH",
                        column: x => x.MaDH,
                        principalTable: "DonHangs",
                        principalColumn: "MaDH");
                    table.ForeignKey(
                        name: "FK_DanhGias_SanPhams_MaSP",
                        column: x => x.MaSP,
                        principalTable: "SanPhams",
                        principalColumn: "MaSP");
                    table.ForeignKey(
                        name: "FK_DanhGias_TaiKhoans_MaTK",
                        column: x => x.MaTK,
                        principalTable: "TaiKhoans",
                        principalColumn: "MaTK");
                });

            migrationBuilder.CreateTable(
                name: "GiaoDichThanhToans",
                columns: table => new
                {
                    MaGiaoDich = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDH = table.Column<int>(type: "int", nullable: false),
                    MaPT = table.Column<int>(type: "int", nullable: false),
                    MaGiaoDichDoiTac = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    ThoiGianThanhToan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThaiGiaoDich = table.Column<int>(type: "int", nullable: false),
                    NoiDungChuyenKhoan = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaoDichThanhToans", x => x.MaGiaoDich);
                    table.ForeignKey(
                        name: "FK_GiaoDichThanhToans_DonHangs_MaDH",
                        column: x => x.MaDH,
                        principalTable: "DonHangs",
                        principalColumn: "MaDH",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GiaoDichThanhToans_PhuongThucThanhToans_MaPT",
                        column: x => x.MaPT,
                        principalTable: "PhuongThucThanhToans",
                        principalColumn: "MaPT",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDonHangs",
                columns: table => new
                {
                    MaDH = table.Column<int>(type: "int", nullable: false),
                    MaBienThe = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGiaXuat = table.Column<decimal>(type: "decimal(18,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDonHangs", x => new { x.MaDH, x.MaBienThe });
                    table.ForeignKey(
                        name: "FK_ChiTietDonHangs_BienTheSanPhams_MaBienThe",
                        column: x => x.MaBienThe,
                        principalTable: "BienTheSanPhams",
                        principalColumn: "MaBienThe",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHangs_DonHangs_MaDH",
                        column: x => x.MaDH,
                        principalTable: "DonHangs",
                        principalColumn: "MaDH",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietGioHangs",
                columns: table => new
                {
                    MaGH = table.Column<int>(type: "int", nullable: false),
                    MaBienThe = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietGioHangs", x => new { x.MaGH, x.MaBienThe });
                    table.ForeignKey(
                        name: "FK_ChiTietGioHangs_BienTheSanPhams_MaBienThe",
                        column: x => x.MaBienThe,
                        principalTable: "BienTheSanPhams",
                        principalColumn: "MaBienThe",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietGioHangs_GioHangs_MaGH",
                        column: x => x.MaGH,
                        principalTable: "GioHangs",
                        principalColumn: "MaGH",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BienTheSanPhams_MaSP",
                table: "BienTheSanPhams",
                column: "MaSP");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHangs_MaBienThe",
                table: "ChiTietDonHangs",
                column: "MaBienThe");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietGioHangs_MaBienThe",
                table: "ChiTietGioHangs",
                column: "MaBienThe");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGias_MaDH",
                table: "DanhGias",
                column: "MaDH");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGias_MaSP",
                table: "DanhGias",
                column: "MaSP");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGias_MaTK",
                table: "DanhGias",
                column: "MaTK");

            migrationBuilder.CreateIndex(
                name: "IX_DonHangs_MaTK",
                table: "DonHangs",
                column: "MaTK");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoDichThanhToans_MaDH",
                table: "GiaoDichThanhToans",
                column: "MaDH");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoDichThanhToans_MaPT",
                table: "GiaoDichThanhToans",
                column: "MaPT");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangs_MaTK",
                table: "GioHangs",
                column: "MaTK");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_MaDM",
                table: "SanPhams",
                column: "MaDM");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietDonHangs");

            migrationBuilder.DropTable(
                name: "ChiTietGioHangs");

            migrationBuilder.DropTable(
                name: "DanhGias");

            migrationBuilder.DropTable(
                name: "GiaoDichThanhToans");

            migrationBuilder.DropTable(
                name: "KhachHangs");

            migrationBuilder.DropTable(
                name: "QuanTriViens");

            migrationBuilder.DropTable(
                name: "BienTheSanPhams");

            migrationBuilder.DropTable(
                name: "GioHangs");

            migrationBuilder.DropTable(
                name: "DonHangs");

            migrationBuilder.DropTable(
                name: "PhuongThucThanhToans");

            migrationBuilder.DropTable(
                name: "SanPhams");

            migrationBuilder.DropTable(
                name: "TaiKhoans");

            migrationBuilder.DropTable(
                name: "DanhMucs");
        }
    }
}
