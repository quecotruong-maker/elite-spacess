
using System.ComponentModel.DataAnnotations;

namespace QuanLyKhachSan.Models
{
    public class DoiPhongViewModel
    {
        public int MaDatPhong { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phòng mới.")]
        [Display(Name = "Phòng mới")]
        public int MaPhongMoi { get; set; }

        [StringLength(500, ErrorMessage = "Lý do không được vượt quá 500 ký tự.")]
        [Display(Name = "Lý do đổi phòng")]
        public string? LyDo { get; set; }

        public string SoPhongCu { get; set; } = string.Empty;

        public string TenKhachHang { get; set; } = string.Empty;

        public DateTime NgayDen { get; set; }

        public DateTime NgayDi { get; set; }

        public string TrangThaiDatPhong { get; set; } = string.Empty;
    }
}
