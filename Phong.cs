
using System.ComponentModel.DataAnnotations;

namespace QuanLyKhachSan.Models
{
    public class Phong
    {
        [Key]
        public int MaPhong { get; set; }

        [Required, StringLength(20)]
        [Display(Name = "Số phòng")]
        public string SoPhong { get; set; } = string.Empty;

        [Display(Name = "Loại phòng")]
        public int MaLoaiPhong { get; set; }
        public LoaiPhong? LoaiPhong { get; set; }

        // Trống / Đã đặt / Đang ở / Bảo trì
        [Required, StringLength(30)]
        [Display(Name = "Trạng thái phòng")]
        public string TrangThai { get; set; } = "Trống";

        // Bẩn / Sạch / Đã kiểm tra
        [Required, StringLength(30)]
        [Display(Name = "Tình trạng vệ sinh")]
        public string TinhTrangVeSinh { get; set; } = "Đã kiểm tra";

        [Display(Name = "Ảnh phòng")]
        [StringLength(255)]
        public string? HinhAnh { get; set; }
    }
}
