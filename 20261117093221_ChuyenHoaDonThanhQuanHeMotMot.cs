using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyKhachSan.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChuyenHoaDonThanhQuanHeMotMot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HoaDon_MaDatPhong",
                table: "HoaDon");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MaDatPhong",
                table: "HoaDon",
                column: "MaDatPhong",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HoaDon_MaDatPhong",
                table: "HoaDon");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MaDatPhong",
                table: "HoaDon",
                column: "MaDatPhong");
        }
    }
}
