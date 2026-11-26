using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyKhachSan.Data.Migrations
{
    /// <inheritdoc />
    public partial class ThemLichSuDoiPhong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LichSuDatPhongs",
                columns: table => new
                {
                    MaLichSu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDatPhong = table.Column<int>(type: "int", nullable: false),
                    MaPhongCu = table.Column<int>(type: "int", nullable: false),
                    SoPhongCu = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaPhongMoi = table.Column<int>(type: "int", nullable: false),
                    SoPhongMoi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NguoiThucHienId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    TenNguoiThucHien = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ThoiGianThayDoi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuDatPhongs", x => x.MaLichSu);
                    table.ForeignKey(
                        name: "FK_LichSuDatPhongs_DatPhong_MaDatPhong",
                        column: x => x.MaDatPhong,
                        principalTable: "DatPhong",
                        principalColumn: "MaDatPhong",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LichSuDatPhongs_MaDatPhong",
                table: "LichSuDatPhongs",
                column: "MaDatPhong");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LichSuDatPhongs");
        }
    }
}
