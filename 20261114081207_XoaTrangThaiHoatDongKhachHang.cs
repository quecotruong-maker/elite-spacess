using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyKhachSan.Data.Migrations
{
    /// <inheritdoc />
    public partial class XoaTrangThaiHoatDongKhachHang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DangHoatDong",
                table: "KhachHang");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DangHoatDong",
                table: "KhachHang",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
