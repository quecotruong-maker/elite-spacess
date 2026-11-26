using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyKhachSan.Data.Migrations
{
    /// <inheritdoc />
    public partial class ThemLienKetTaiKhoanKhachHang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityUserId",
                table: "KhachHang",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KhachHang_IdentityUserId",
                table: "KhachHang",
                column: "IdentityUserId",
                unique: true,
                filter: "[IdentityUserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_KhachHang_AspNetUsers_IdentityUserId",
                table: "KhachHang",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KhachHang_AspNetUsers_IdentityUserId",
                table: "KhachHang");

            migrationBuilder.DropIndex(
                name: "IX_KhachHang_IdentityUserId",
                table: "KhachHang");

            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "KhachHang");
        }
    }
}
