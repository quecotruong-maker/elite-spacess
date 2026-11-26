using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyKhachSan.Data.Migrations
{
    /// <inheritdoc />
    public partial class SuaQuanHePhongLoaiPhong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Phong_LoaiPhong_LoaiPhongMaLoaiPhong",
                table: "Phong");

            migrationBuilder.DropIndex(
                name: "IX_Phong_LoaiPhongMaLoaiPhong",
                table: "Phong");

            migrationBuilder.DropColumn(
                name: "LoaiPhongMaLoaiPhong",
                table: "Phong");

            migrationBuilder.CreateIndex(
                name: "IX_Phong_MaLoaiPhong",
                table: "Phong",
                column: "MaLoaiPhong");

            migrationBuilder.AddForeignKey(
                name: "FK_Phong_LoaiPhong_MaLoaiPhong",
                table: "Phong",
                column: "MaLoaiPhong",
                principalTable: "LoaiPhong",
                principalColumn: "MaLoaiPhong",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Phong_LoaiPhong_MaLoaiPhong",
                table: "Phong");

            migrationBuilder.DropIndex(
                name: "IX_Phong_MaLoaiPhong",
                table: "Phong");

            migrationBuilder.AddColumn<int>(
                name: "LoaiPhongMaLoaiPhong",
                table: "Phong",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Phong_LoaiPhongMaLoaiPhong",
                table: "Phong",
                column: "LoaiPhongMaLoaiPhong");

            migrationBuilder.AddForeignKey(
                name: "FK_Phong_LoaiPhong_LoaiPhongMaLoaiPhong",
                table: "Phong",
                column: "LoaiPhongMaLoaiPhong",
                principalTable: "LoaiPhong",
                principalColumn: "MaLoaiPhong");
        }
    }
}
