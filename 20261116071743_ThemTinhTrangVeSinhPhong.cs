
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyKhachSan.Data.Migrations
{
    public partial class ThemTinhTrangVeSinhPhong : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Thêm cột tình trạng vệ sinh
            migrationBuilder.AddColumn<string>(
                name: "TinhTrangVeSinh",
                table: "Phong",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Đã kiểm tra");

            // Chuyển phòng đang dọn sang trạng thái vệ sinh Bẩn
            migrationBuilder.Sql(@"
                UPDATE Phong
                SET TinhTrangVeSinh = N'Bẩn',
                    TrangThai = N'Trống'
                WHERE TrangThai = N'Đang dọn';
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Khôi phục trạng thái Đang dọn trước khi xóa cột
            migrationBuilder.Sql(@"
                UPDATE Phong
                SET TrangThai = N'Đang dọn'
                WHERE TinhTrangVeSinh = N'Bẩn'
                  AND TrangThai = N'Trống';
            ");

            migrationBuilder.DropColumn(
                name: "TinhTrangVeSinh",
                table: "Phong");
        }
    }
}
